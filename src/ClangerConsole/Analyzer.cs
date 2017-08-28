using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClangSharp;
using System.Runtime.InteropServices;

namespace ClangerConsole {
	public class Analyzer : IDisposable {
		const string NameScopeDelimiter = ".";

		#region PInvokes
		[DllImport("libclang", CallingConvention = CallingConvention.Cdecl, EntryPoint = "clang_Cursor_getOperatorString")]
		public static extern CXString Cursor_getOperatorString(CXCursor param0);
		[DllImport("libclang", CallingConvention = CallingConvention.Cdecl, EntryPoint = "clang_Cursor_getLiteralString")]
		public static extern CXString Cursor_getLiteralString(CXCursor param0);
		#endregion

		#region クラスなど
		public struct TranslationUnitKey {
			public CXTranslationUnit TU;

			public TranslationUnitKey(CXTranslationUnit tu) {
				this.TU = tu;
			}

			public override bool Equals(object obj) {
				if (obj is TranslationUnitKey)
					return (TranslationUnitKey)obj == this;
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return this.TU.Pointer.GetHashCode();
			}

			public static bool operator ==(TranslationUnitKey a, TranslationUnitKey b) {
				return a.TU.Pointer == b.TU.Pointer;
			}

			public static bool operator !=(TranslationUnitKey a, TranslationUnitKey b) {
				return a.TU.Pointer != b.TU.Pointer;
			}
		}

		public struct EntityKey {
			public CXSourceLocation Loc;

			public EntityKey(CXSourceLocation loc) {
				this.Loc = loc;
			}

			public override bool Equals(object obj) {
				if (obj is EntityKey)
					return (EntityKey)obj == this;
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return this.Loc.ptr_data0.GetHashCode() ^ this.Loc.ptr_data1.GetHashCode() ^ this.Loc.int_data.GetHashCode();
			}

			public static bool operator ==(EntityKey a, EntityKey b) {
				if (a.Loc.ptr_data0 != b.Loc.ptr_data0)
					return false;
				if (a.Loc.ptr_data1 != b.Loc.ptr_data1)
					return false;
				if (a.Loc.int_data != b.Loc.int_data)
					return false;
				return true;
			}

			public static bool operator !=(EntityKey a, EntityKey b) {
				if (a.Loc.ptr_data0 != b.Loc.ptr_data0)
					return true;
				if (a.Loc.ptr_data1 != b.Loc.ptr_data1)
					return true;
				if (a.Loc.int_data != b.Loc.int_data)
					return true;
				return false;
			}
		}

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

		public enum ScopeKind {
			File,
			Namespace,
			Class,
			Function,
			Block,
		}

		public class NamedItem {
			public Scope Parent;
			public string Name;

			public string FullName {
				get {
					var sb = new StringBuilder();
					foreach (var s in this.Path) {
						var name = s.Name;
						if (sb.Length != 0 && name.Length != 0)
							sb.Append(NameScopeDelimiter);
						sb.Append(name);
					}
					return sb.ToString();
				}
			}

			public NamedItem[] Path {
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
			CXCursor Cursor;

			public Location ExpansionLocation => new Location(this.Cursor, Location.Kind.Expansion);
			public Location PresumedLocation => new Location(this.Cursor, Location.Kind.Presumed);
			public Location SpellingLocation => new Location(this.Cursor, Location.Kind.Spelling);
			public Location FileLocation => new Location(this.Cursor, Location.Kind.File);

			public Entity(Scope parent, string name, CXCursor cursor)
				: base(parent, name) {
				this.Parent = parent;
				this.Name = name;
				this.Cursor = cursor;
			}
		}

		public class Scope : NamedItem {
			public Dictionary<string, NamedItem> Children;
			public ScopeKind Kind;

			public Scope(Scope parent, string name, ScopeKind kind)
				: base (parent, name) {
				this.Parent = parent;
				this.Name = name;
				this.Kind = kind;
			}

			public Scope ChildScope(string name, ScopeKind kind) {
				NamedItem ni;
				if (this.Children == null)
					this.Children = new Dictionary<string, NamedItem>();
				Scope scope;
				if (this.Children.TryGetValue(name, out ni)) {
					scope = ni as Scope;
					if (scope == null)
						throw new ApplicationException(string.Concat("\"", name, "\" class mismatch: ", ni.GetType().Name, " vs Scope"));
					if (scope.Kind != kind)
						throw new ApplicationException(string.Concat("\"", name, "\" Scope kind mismatch: " , scope.Kind, " vs ", kind));
					return scope;
				}
				scope = new Scope(this, name, kind);
				this.Children[name] = scope;
				return scope;
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

		//public class Entity {
		//	public CXCursor Cursor;
		//	public ScopePath ParentPath;

		//	public Location ExpansionLocation => new Location(this.Cursor, Location.Kind.Expansion);
		//	public Location PresumedLocation => new Location(this.Cursor, Location.Kind.Presumed);
		//	public Location SpellingLocation => new Location(this.Cursor, Location.Kind.Spelling);
		//	public Location FileLocation => new Location(this.Cursor, Location.Kind.File);

		//	public Entity(CXCursor cursor, ScopePath parentPath) {
		//		this.Cursor = cursor;
		//		this.ParentPath = parentPath;
		//	}
		//}

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

		public struct Position {
			public uint Line;
			public uint Column;

			public override int GetHashCode() {
				return this.Line.GetHashCode() ^ this.Column.GetHashCode();
			}

			public override bool Equals(object obj) {
				if (obj is Position)
					return (Position)obj == this;
				return base.Equals(obj);
			}

			public static bool operator ==(Position a, Position b) {
				return a.Line == b.Line && a.Column == b.Column;
			}

			public static bool operator !=(Position a, Position b) {
				return a.Line != b.Line || a.Column != b.Column;
			}
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

		#region フィールド
		CXIndex _Index;
		Dictionary<string, CXTranslationUnit> _TranslationUnits = new Dictionary<string, CXTranslationUnit>();
		Dictionary<EntityKey, Entity> _Entities = new Dictionary<EntityKey, Entity>();
		ScopePath _ScopePath = new ScopePath();
		Dictionary<TypeKey, Type> _Types = new Dictionary<TypeKey, Type>();
		#endregion

		#region 公開メソッド
		public Analyzer() {
			_Index = clang.createIndex(0, 1);
		}

		public void Parse(string sourceFile, string[] includeDirs = null, string[] additionalOptions = null, bool msCompati = true) {
			var fullPath = System.IO.Path.GetFullPath(sourceFile.ToLower());
			if (_TranslationUnits.ContainsKey(fullPath))
				return;

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

			var erro = clang.parseTranslationUnit2(_Index, sourceFile, options.ToArray(), options.Count, out ufile, 0, 0, out tu);
			var diags = Diagnostic.CreateFrom(tu);

			// 本当はここでエラーチェックが好ましい
			var cursor = clang.getTranslationUnitCursor(tu);

			clang.visitChildren(cursor, this.VisitChild, new CXClientData());


			_TranslationUnits[fullPath] = tu;
		}
		#endregion

		#region 内部メソッド
		public Type TypeOf(CXCursor cursor) {
			cursor = clang.getCanonicalCursor(cursor);

			Entity ent;
			var ekey = new EntityKey(clang.getCursorLocation(cursor));
			if (_Entities.TryGetValue(ekey, out ent))
				return ent as Type;

			var type = clang.getCursorType(cursor);
			type = clang.getCanonicalType(type);

			var tkey = new TypeKey(type);
			var typeEnt = new Type(cursor, _ScopePath.Clone(), type);

			_Entities[ekey] = typeEnt;
			_Types[tkey] = typeEnt;

			return typeEnt;
		}

		public Type TypeOf(CXType type) {
			return TypeOf(clang.getTypeDeclaration(type));
		}

		public Variable VariableOf(CXCursor cursor) {
			cursor = clang.getCanonicalCursor(cursor);

			Entity ent;
			var ekey = new EntityKey(clang.getCursorLocation(cursor));
			if (_Entities.TryGetValue(ekey, out ent))
				return ent as Variable;

			var varEnt = new Variable(cursor, _ScopePath.Clone());

			_Entities[ekey] = varEnt;

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

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
				}

				// TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// TODO: 大きなフィールドを null に設定します。
				foreach (var tu in _TranslationUnits.Values) {
					clang.disposeTranslationUnit(tu);
				}
				_TranslationUnits.Clear();
				clang.disposeIndex(_Index);

				disposedValue = true;
			}
		}

		~Analyzer() {
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(false);
		}

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void Dispose() {
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
