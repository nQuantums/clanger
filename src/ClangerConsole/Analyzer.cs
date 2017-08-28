using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClangSharp;
using System.Runtime.InteropServices;

namespace ClangerConsole {
	public class Analyzer {
		const string NameScopeDelimiter = "::";

		#region PInvokes
		[DllImport("libclang", CallingConvention = CallingConvention.Cdecl, EntryPoint = "clang_Cursor_getOperatorString")]
		public static extern CXString Cursor_getOperatorString(CXCursor param0);
		[DllImport("libclang", CallingConvention = CallingConvention.Cdecl, EntryPoint = "clang_Cursor_getLiteralString")]
		public static extern CXString Cursor_getLiteralString(CXCursor param0);
		#endregion

		#region クラスなど
		public struct TypeKey {
			public IntPtr data0;
			public IntPtr data1;

			public TypeKey(CXType type) {
				this.data0 = type.data0;
				this.data1 = type.data1;
			}

			public override bool Equals(object obj) {
				if (obj is TypeKey)
					return (TypeKey)obj == this;
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return this.data0.GetHashCode() ^ this.data1.GetHashCode();
			}

			public static bool operator ==(TypeKey a, TypeKey b) {
				if (a.data0 != b.data0)
					return false;
				if (a.data1 != b.data1)
					return false;
				return true;
			}

			public static bool operator !=(TypeKey a, TypeKey b) {
				if (a.data0 != b.data0)
					return true;
				if (a.data1 != b.data1)
					return true;
				return false;
			}
		}

		public struct CursorKey {
			public IntPtr data0;
			public IntPtr data1;
			public IntPtr data2;

			public CursorKey(CXCursor c) {
				this.data0 = c.data0;
				this.data1 = c.data1;
				this.data2 = c.data2;
			}

			public override bool Equals(object obj) {
				if (obj is CursorKey)
					return (CursorKey)obj == this;
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return this.data0.GetHashCode() ^ this.data1.GetHashCode() ^ this.data2.GetHashCode();
			}

			public static bool operator ==(CursorKey a, CursorKey b) {
				if (a.data0 != b.data0)
					return false;
				if (a.data1 != b.data1)
					return false;
				if (a.data2 != b.data2)
					return false;
				return true;
			}

			public static bool operator !=(CursorKey a, CursorKey b) {
				if (a.data0 != b.data0)
					return true;
				if (a.data1 != b.data1)
					return true;
				if (a.data2 != b.data2)
					return true;
				return false;
			}
		}

		public class NamedItem {
			public Scope Parent;
			public string Name;

			public string Path {
				get {
					var sb = new StringBuilder();
					foreach (var ni in this.PathItems) {
						var name = ni.Name;
						if (sb.Length != 0 && name.Length != 0)
							sb.Append(NameScopeDelimiter);
						sb.Append(name);
					}
					return sb.ToString();
				}
			}

			public NamedItem[] PathItems {
				get {
					var path = new List<NamedItem>();
					var ni = this;
					do {
						path.Add(ni);
						ni = ni.Parent;
					} while (ni != null);
					path.Reverse();
					return path.ToArray();
				}
			}

			public NamedItem(Scope parent, string name) {
				this.Parent = parent;
				this.Name = name;
			}
		}

		public class Entity : NamedItem {
			public CXCursor Cursor;

			public Location Location => new Location(this.Cursor);

			public Entity(Scope parent, string name, CXCursor cursor)
				: base(parent, name) {
				this.Cursor = cursor;
			}
		}


		public enum ScopeKind {
			Namespace,
			Class,
			Function,
			Block,
		}

		public class Scope : NamedItem {
			public ScopeKind Kind;
			public Dictionary<string, NamedItem> Children;

			public Scope(Scope parent, string name, ScopeKind kind)
				: base(parent, name) {
				this.Kind = kind;
			}

			public Scope ChildScope(string name, ScopeKind kind) {
				if (this.Children == null)
					this.Children = new Dictionary<string, NamedItem>();

				NamedItem ni;
				Scope scope;
				if (this.Children.TryGetValue(name, out ni)) {
					scope = ni as Scope;
					if (scope == null)
						throw new ApplicationException();
					if(scope.Kind != kind)
						throw new ApplicationException();
					return scope;
				} else {
					scope = new Scope(this, name, kind);
					this.Children[name] = scope;
					return scope;
				}
			}
		}

		public class ScopePath {
			List<Scope> _Scopes = new List<Scope>();

			public string Name {
				get {
					if (_Scopes.Count == 0)
						return "";
					return _Scopes[_Scopes.Count - 1].Name;
				}
			}

			public string FullName {
				get {
					var sb = new StringBuilder();
					foreach (var ns in _Scopes) {
						var name = ns.Name;
						if (sb.Length != 0 && name.Length != 0)
							sb.Append(NameScopeDelimiter);
						sb.Append(name);
					}
					return sb.ToString();
				}
			}

			public string Push(Scope ns) {
				_Scopes.Add(ns);
				return this.FullName;
			}

			public void Pop() {
				if (_Scopes.Count == 0)
					return;
				_Scopes.RemoveAt(_Scopes.Count - 1);
			}

			public string MakeFullName(string name) {
				var parentPath = this.FullName;
				return string.IsNullOrEmpty(parentPath) ? name : string.Concat(parentPath, NameScopeDelimiter, name);
			}

			public ScopePath Clone() {
				var c = this.MemberwiseClone() as ScopePath;
				c._Scopes = new List<Scope>(c._Scopes);
				return c;
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Type : Entity {
			public CXType ClangType;
			public string Name;

			public string FullName => this.ParentPath != null ? this.ParentPath.MakeFullName(this.Name) : Name;
			public CXType Canonical => clang.getCanonicalType(this.ClangType);
			public long AlignOf => clang.Type_getAlignOf(this.ClangType);
			public CXType ClassType => clang.Type_getClassType(this.ClangType);
			public CXRefQualifierKind RefQualifier => clang.Type_getCXXRefQualifier(this.ClangType);
			public int NumTemplateArguments => clang.Type_getNumTemplateArguments(this.ClangType);
			public long SizeOf => clang.Type_getSizeOf(this.ClangType);
			public CXCursor Declaration => clang.getTypeDeclaration(this.ClangType);

			public Type(CXCursor cursor, ScopePath parentPath, CXType type)
				: base(cursor, parentPath) {
				this.ClangType = type;
				this.Name = clang.getCursorDisplayName(cursor).ToString();
			}

			public long OffsetOf(string S) {
				return clang.Type_getOffsetOf(this.ClangType, S);

			}

			//public static ClangType TypedefDeclUnderlyingType(CXCursor c) {
			//	return new ClangType(clang.getTypedefDeclUnderlyingType(c));
			//}

			public static string KindSpelling(CXTypeKind K) {
				return clang.getTypeKindSpelling(K).ToString();
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Variable : Entity {
			public CXType Type;
			public string Name;

			public string FullName => this.ParentPath != null ? this.ParentPath.MakeFullName(this.Name) : Name;

			public Variable(CXCursor cursor, ScopePath parentPath)
				: base(cursor, parentPath) {
				this.Type = clang.getCursorType(cursor);
				this.Name = clang.getCursorDisplayName(cursor).ToString();
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Diagnostic {
			public uint Category;
			public string CategoryName {
				get {
					return clang.getDiagnosticCategoryName(this.Category).ToString();
				}
			}
			public string CategoryText;
			public string Text;
			public Location Location;

			public Diagnostic(CXDiagnostic d) {
				this.Category = clang.getDiagnosticCategory(d);
				this.CategoryText = clang.getDiagnosticCategoryText(d).ToString();
				this.Text = clang.formatDiagnostic(d, unchecked((uint)-1)).ToString();
				this.Location = new Location(clang.getDiagnosticLocation(d), Location.Kind.Presumed);
			}

			public static Diagnostic[] CreateFrom(CXTranslationUnit tu) {
				var n = clang.getNumDiagnostics(tu);
				var diags = new Diagnostic[n];
				for (int i = 0; i < diags.Length; i++)
					diags[i] = new Diagnostic(clang.getDiagnostic(tu, (uint)i));
				return diags;
			}

			public override string ToString() {
				return this.Text;
			}

			// TODO: clang.disposeDiagnostic 呼び出す必要ありそう
		}

		public class Location {
			public enum Kind {
				Expansion,
				Presumed,
				Spelling,
				File,
			}

			public string File;
			public string FullPath;
			public uint Line;
			public uint Column;
			public uint Offset;

			public Location(CXSourceLocation loc, Kind kind = Kind.Presumed) {
				string file;
				uint line, column, offset = 0;

				switch (kind) {
				case Kind.Expansion: {
						CXFile f;
						clang.getExpansionLocation(loc, out f, out line, out column, out offset);
						file = clang.getFileName(f).ToString();
					}
					break;
				case Kind.Presumed: {
						CXString f;
						clang.getPresumedLocation(loc, out f, out line, out column);
						file = f.ToString();
					}
					break;
				case Kind.Spelling: {
						CXFile f;
						clang.getSpellingLocation(loc, out f, out line, out column, out offset);
						file = clang.getFileName(f).ToString();
					}
					break;
				case Kind.File: {
						CXFile f;
						clang.getFileLocation(loc, out f, out line, out column, out offset);
						file = clang.getFileName(f).ToString();
					}
					break;
				default:
					throw new NotImplementedException();
				}

				this.File = file;
				this.FullPath = this.File != null ? System.IO.Path.GetFullPath(this.File) : null;
				this.Line = line;
				this.Column = column;
				this.Offset = offset;
			}

			public Location(CXCursor c, Kind kind = Kind.Presumed)
				: this(clang.getCursorLocation(c), kind) {
			}

			public override string ToString() {
				return string.Concat("<\"", this.FullPath, "\">(", this.Line, ", ", this.Column, ")");
			}
		}
		#endregion

		Dictionary<CursorKey, Entity> _Entities = new Dictionary<CursorKey, Entity>();
		ScopePath _ScopePath = new ScopePath();
		Dictionary<TypeKey, Type> _Types = new Dictionary<TypeKey, Type>();

		#region 公開メソッド
		public void Parse(string sourceFile, string[] includeDirs = null, string[] additionalOptions = null, bool msCompati = true) {
			var index = clang.createIndex(1, 1);
			var ufile = new CXUnsavedFile();
			var tu = new CXTranslationUnit();
			var options = new List<string>(new string[] { "-std=c++11", "-ferror-limit=9999" });

			if (includeDirs != null) {
				foreach (var dir in includeDirs) {
					options.Add(string.Concat("-I", dir, ""));
				}
			}

			if (additionalOptions != null) {
				options.AddRange(additionalOptions);
			}

			if (msCompati) {
				options.Add("-fms-extensions");
				options.Add("-fms-compatibility");
			}

			var erro = clang.parseTranslationUnit2(index, sourceFile, options.ToArray(), options.Count, out ufile, 0, 0, out tu);
			var diags = Diagnostic.CreateFrom(tu);

			// 本当はここでエラーチェックが好ましい
			var cursor = clang.getTranslationUnitCursor(tu);

			clang.visitChildren(cursor, this.VisitChild, new CXClientData());
		}
		#endregion

		#region 内部メソッド
		public Type TypeOf(CXCursor cursor) {
			cursor = clang.getCanonicalCursor(cursor);

			Entity ent;
			var ckey = new CursorKey(cursor);
			if (_Entities.TryGetValue(ckey, out ent))
				return ent as Type;

			var type = clang.getCursorType(cursor);
			type = clang.getCanonicalType(type);

			var tkey = new TypeKey(type);
			var typeEnt = new Type(cursor, _ScopePath.Clone(), type);

			_Entities[ckey] = typeEnt;
			_Types[tkey] = typeEnt;

			return typeEnt;
		}

		public Type TypeOf(CXType type) {
			return TypeOf(clang.getTypeDeclaration(type));
		}

		public Variable VariableOf(CXCursor cursor) {
			cursor = clang.getCanonicalCursor(cursor);

			Entity ent;
			var ckey = new CursorKey(cursor);
			if (_Entities.TryGetValue(ckey, out ent))
				return ent as Variable;

			var varEnt = new Variable(cursor, _ScopePath.Clone());

			_Entities[ckey] = varEnt;

			return varEnt;
		}

		public Scope ScopeOf(CXCursor cursor) {
			return new Scope(clang.getCursorDisplayName(cursor).ToString(), _ScopePath.FullName, null);
		}

		CXChildVisitResult VisitChild(CXCursor cursor, CXCursor parent, IntPtr client_data) {
			var dname = clang.getCursorDisplayName(cursor).ToString();
			if (!string.IsNullOrEmpty(dname) && dname.Contains("TemplMethodInTemplVar")) {
				var loc1 = new Location(clang.getCursorReferenced(cursor), Location.Kind.Expansion);
				var loc2 = new Location(clang.getCursorReferenced(cursor), Location.Kind.Presumed);
				var loc3 = new Location(clang.getCursorReferenced(cursor), Location.Kind.Spelling);
				var loc4 = new Location(clang.getCursorReferenced(cursor), Location.Kind.File);
			}

			switch (cursor.kind) {
			case CXCursorKind.CXCursor_TypedefDecl:
				TypeOf(cursor);
				break;
			case CXCursorKind.CXCursor_StructDecl:
			case CXCursorKind.CXCursor_ClassDecl:
			case CXCursorKind.CXCursor_ClassTemplate:
				TypeOf(cursor);
				_ScopePath.Push(ScopeOf(cursor));
				try {
					clang.visitChildren(cursor, this.VisitChild, new CXClientData());
				} finally {
					_ScopePath.Pop();
				}
				break;

			case CXCursorKind.CXCursor_FunctionDecl:
			case CXCursorKind.CXCursor_FunctionTemplate:
			case CXCursorKind.CXCursor_CXXMethod:
				_ScopePath.Push(ScopeOf(cursor));
				try {
					clang.visitChildren(cursor, this.VisitChild, new CXClientData());
				} finally {
					_ScopePath.Pop();
				}
				break;

			case CXCursorKind.CXCursor_VarDecl:
			case CXCursorKind.CXCursor_FieldDecl:
				VariableOf(cursor);
				break;

			case CXCursorKind.CXCursor_BinaryOperator: {
					var op = Cursor_getOperatorString(cursor).ToString();
				}
				break;
			case CXCursorKind.CXCursor_Namespace:
				_ScopePath.Push(ScopeOf(cursor));
				try {
					clang.visitChildren(cursor, this.VisitChild, new CXClientData());
				} finally {
					_ScopePath.Pop();
				}
				break;
			default:
				break;
			}

			return CXChildVisitResult.CXChildVisit_Recurse;
		}
		#endregion
	}
}
