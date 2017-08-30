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
			public CXCursor Cursor;

			public CursorKey(CXCursor c) {
				this.Cursor = c;
			}

			public override bool Equals(object obj) {
				if (obj is CursorKey)
					return (CursorKey)obj == this;
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return this.Cursor.data0.GetHashCode() ^ this.Cursor.GetHashCode() ^ this.Cursor.GetHashCode();
			}

			public static bool operator ==(CursorKey a, CursorKey b) {
				if (a.Cursor.data0 != b.Cursor.data0)
					return false;
				if (a.Cursor.data1 != b.Cursor.data1)
					return false;
				if (a.Cursor.data2 != b.Cursor.data2)
					return false;
				return true;
			}

			public static bool operator !=(CursorKey a, CursorKey b) {
				if (a.Cursor.data0 != b.Cursor.data0)
					return true;
				if (a.Cursor.data1 != b.Cursor.data1)
					return true;
				if (a.Cursor.data2 != b.Cursor.data2)
					return true;
				return false;
			}
		}

		public class File {
			public string FullName;
			public Dictionary<Position, Entity> Entities = new Dictionary<Position, Entity>();

			public File(string fullName) {
				this.FullName = fullName;
			}
		}

		public class FileContainer {
			Dictionary<string, File> _FilesDic = new Dictionary<string, File>();

			public FileContainer() {
			}

			public File ChildFile(string fileName) {
				var fullName = System.IO.Path.GetFullPath(fileName).ToLower();
				File file;
				if (_FilesDic.TryGetValue(fullName, out file))
					return file;
				file = new File(fullName);
				_FilesDic[fullName] = file;
				return file;
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Position {
			[FieldOffset(0)]
			public uint Line;
			[FieldOffset(4)]
			public uint Column;
			[FieldOffset(0)]
			public ulong Value;

			public override int GetHashCode() {
				return this.Value.GetHashCode();
			}

			public override bool Equals(object obj) {
				if (obj is Position)
					return (Position)obj == this;
				return base.Equals(obj);
			}

			public static bool operator ==(Position a, Position b) {
				return a.Value == b.Value;
			}

			public static bool operator !=(Position a, Position b) {
				return a.Value != b.Value;
			}
		}

		/// <summary>
		/// 名前付きスコープ
		/// </summary>
		public class Scope {
			public Scope Parent;
			public string Name;
			public Dictionary<string, Scope> Children;

			public string FullName {
				get {
					var sb = new StringBuilder();
					foreach (var s in this.Path) {
						var name = s.Name;
						if (string.IsNullOrEmpty(name))
							continue;

						if (sb.Length != 0)
							sb.Append(NameScopeDelimiter);
						sb.Append(name);
					}
					return sb.ToString();
				}
			}

			public Scope[] Path {
				get {
					var path = new List<Scope>();
					var ni = this;
					do {
						path.Add(ni);
						ni = ni.Parent;
					} while (ni != null);
					path.Reverse();
					return path.ToArray();
				}
			}

			public Scope(Scope parent, string name) {
				this.Parent = parent;
				this.Name = name;
			}

			protected static T Child<T>(Scope parent, File file, string name, Func<T> creator) where T : Scope {
				if (parent.Children == null) {
					if (creator == null)
						return null;
					parent.Children = new Dictionary<string, Scope>();
				}

				T t;
				Scope s;
				if (parent.Children.TryGetValue(name, out s)) {
					// 同名のスコープパスが既に存在しているなら
					// 既存のものを取得する、もし異なるアイテムが衝突しているなら同名の衝突アイテムとして登録して取得する
					var e = s as Entity;
					var f = e?.File;

					// スコープパスとファイルが同じなら同じアイテムを指していなければならない
					if (f == file) {
						t = s as T;
						if (t == null)
							throw new ApplicationException(string.Concat("\"", name, "\" class mismatch: ", s.GetType().Name, " vs ", typeof(T).Name));
						return t;
					}

					// スコープパスが同じでファイルが異なるなら同名のアイテムが衝突している
					var c = s as Conflicts;
					if (c != null) {
						if (c.ConflictsChildren.TryGetValue(file, out s)) {
							// ファイルで絞って取得
							t = s as T;
							if (t == null)
								throw new ApplicationException(string.Concat("\"", name, "\" class mismatch: ", s.GetType().Name, " vs ", typeof(T).Name));
							return t;
						}
					} else {
						if (creator == null)
							return null;

						// 新規衝突アイテムコレクション作成
						c = new Conflicts(parent);
						c.ConflictsChildren[f] = s;
					}

					// 衝突アイテムとして追加
					t = creator();
					c.ConflictsChildren[file] = t;

					return t;
				}

				// 新規アイテムを作成する
				if (creator == null)
					return null;

				t = creator();
				parent.Children[name] = t;

				return t;
			}

			public Namespace ChildNamespace(string name) {
				return Child<Namespace>(this, null, name, () => new Namespace(this, name));
			}

			public Type ChildType(File file, string name, CXCursor cursor) {
				var type = Child<Type>(this, file, name, () => new Type(this, file, name, cursor));
				type.Cursors.Add(new CursorKey(cursor));
				return type;
			}

			public Variable ChildVariable(File file, string name, CXCursor cursor) {
				var variable = Child<Variable>(this, file, name, () => new Variable(this, file, name, cursor));
				variable.Cursors.Add(new CursorKey(cursor));
				return variable;
			}

			public Function ChildFunction(File file, string name, CXCursor cursor) {
				var function = Child<Function>(this, file, name, () => new Function(this, file, name, cursor));
				function.Cursors.Add(new CursorKey(cursor));
				return function;
			}
		}

		public class Conflicts : Scope {
			public Dictionary<File, Scope> ConflictsChildren = new Dictionary<File, Scope>();

			public Conflicts(Scope parent)
				: base(parent, null) {
			}
		}

		public class Namespace : Scope {
			public Namespace(Scope parent, string name)
				: base(parent, name) {
				this.Parent = parent;
				this.Name = name;
			}
		}

		public class Entity : Scope {
			public File File;
			public HashSet<CursorKey> Cursors = new HashSet<CursorKey>();

			public Location ExpansionLocation => new Location(this.Cursors.First().Cursor, Location.Kind.Expansion);
			public Location PresumedLocation => new Location(this.Cursors.First().Cursor, Location.Kind.Presumed);
			public Location SpellingLocation => new Location(this.Cursors.First().Cursor, Location.Kind.Spelling);
			public Location FileLocation => new Location(this.Cursors.First().Cursor, Location.Kind.File);

			public Entity(Scope parent, File file, string name, CXCursor cursor)
				: base(parent, name) {
				this.File = file;
				this.Cursors.Add(new CursorKey(cursor));
			}
		}

		public class Type : Entity {
			public CXType ClangType => clang.getCursorType(this.Cursors.First().Cursor);
			public CXType Canonical => clang.getCanonicalType(this.ClangType);
			public long AlignOf => clang.Type_getAlignOf(this.ClangType);
			public CXType ClassType => clang.Type_getClassType(this.ClangType);
			public CXRefQualifierKind RefQualifier => clang.Type_getCXXRefQualifier(this.ClangType);
			public int NumTemplateArguments => clang.Type_getNumTemplateArguments(this.ClangType);
			public long SizeOf => clang.Type_getSizeOf(this.ClangType);
			public CXCursor Declaration => clang.getTypeDeclaration(this.ClangType);

			public Type(Scope parent, File file, string name, CXCursor cursor)
				: base(parent, file, name, cursor) {
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
			public CXType Type => clang.getCursorType(this.Cursors.First().Cursor);

			public Variable(Scope parent, File file, string name, CXCursor cursor)
				: base(parent, file, name, cursor) {
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Function : Entity {
			public CXType Type => clang.getCursorResultType(this.Cursors.First().Cursor);

			public Function(Scope parent, File file, string name, CXCursor cursor)
				: base(parent, file, name, cursor) {
			}

			public override string ToString() {
				return this.FullName;
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
					if (_Scopes.Count == 0)
						return "";
					return _Scopes[_Scopes.Count - 1].FullName;
				}
			}

			public Scope Current {
				get {
					if (_Scopes.Count == 0)
						return null;
					return _Scopes[_Scopes.Count - 1];
				}
			}

			public string Push(Scope scope) {
				_Scopes.Add(scope);
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

		public class Translation : IDisposable {
			public CXTranslationUnit TranslationUnit;
			public File SourceFile;
			public Dictionary<CursorKey, Entity> Entities = new Dictionary<CursorKey, Entity>();

			public Translation(CXTranslationUnit tu, File sourceFile) {
				this.TranslationUnit = tu;
				this.SourceFile = sourceFile;
			}

			#region IDisposable Support
			private bool disposedValue = false; // 重複する呼び出しを検出するには

			protected virtual void Dispose(bool disposing) {
				if (!disposedValue) {
					if (disposing) {
						// マネージ状態を破棄します (マネージ オブジェクト)。
					}

					// アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
					// 大きなフィールドを null に設定します。
					clang.disposeTranslationUnit(this.TranslationUnit);
					this.SourceFile = null;
					this.Entities = null;

					disposedValue = true;
				}
			}

			~Translation() {
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
		#endregion

		#region フィールド
		CXIndex _Index;
		FileContainer _Files = new FileContainer();
		Dictionary<File, Translation> _TranslationUnits = new Dictionary<File, Translation>();
		Translation _CurrentTranslation;
		ScopePath _ScopePath = new ScopePath();
		Namespace _RootNamespace = new Namespace(null, null);
		#endregion

		#region 公開メソッド
		public Analyzer() {
			_Index = clang.createIndex(0, 1);
		}

		public void Parse(string sourceFile, string[] includeDirs = null, string[] additionalOptions = null, bool msCompati = true) {
			var file = _Files.ChildFile(sourceFile);
			if (_TranslationUnits.ContainsKey(file))
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

			_ScopePath = new ScopePath();
			_ScopePath.Push(_RootNamespace);
			_CurrentTranslation = new Translation(tu, file);

			var erro = clang.parseTranslationUnit2(_Index, sourceFile, options.ToArray(), options.Count, out ufile, 0, 0, out tu);
			var diags = Diagnostic.CreateFrom(tu);

			// 本当はここでエラーチェックが好ましい
			var cursor = clang.getTranslationUnitCursor(tu);

			clang.visitChildren(cursor, this.VisitChild, new CXClientData());

			_TranslationUnits[file] = _CurrentTranslation;
		}
		#endregion

		#region 内部メソッド
		public Namespace NamespaceOf(CXCursor cursor) {
			return _ScopePath.Current.ChildNamespace(clang.getCursorDisplayName(cursor).ToString());
		}

		public Type TypeOf(CXCursor cursor) {
			CXString fileName;
			Position position = new Position();
			clang.getPresumedLocation(clang.getCursorLocation(cursor), out fileName, out position.Line, out position.Column);

			var file = _Files.ChildFile(fileName.ToString());
			var entity = _ScopePath.Current.ChildType(file, clang.getCursorDisplayName(cursor).ToString(), cursor);

			file.Entities[position] = entity;
			_CurrentTranslation.Entities[new CursorKey(cursor)] = entity;

			return entity;
		}

		public Variable VariableOf(CXCursor cursor) {
			CXString fileName;
			Position position = new Position();
			clang.getPresumedLocation(clang.getCursorLocation(cursor), out fileName, out position.Line, out position.Column);

			var file = _Files.ChildFile(fileName.ToString());
			var entity = _ScopePath.Current.ChildVariable(file, clang.getCursorDisplayName(cursor).ToString(), cursor);

			file.Entities[position] = entity;
			_CurrentTranslation.Entities[new CursorKey(cursor)] = entity;

			return entity;
		}

		public Function FunctionOf(CXCursor cursor) {
			CXString fileName;
			Position position = new Position();
			clang.getPresumedLocation(clang.getCursorLocation(cursor), out fileName, out position.Line, out position.Column);

			var file = _Files.ChildFile(fileName.ToString());
			var entity = _ScopePath.Current.ChildFunction(file, clang.getCursorDisplayName(cursor).ToString(), cursor);

			file.Entities[position] = entity;
			_CurrentTranslation.Entities[new CursorKey(cursor)] = entity;

			return entity;
		}

		CXChildVisitResult VisitChild(CXCursor cursor, CXCursor parent, IntPtr client_data) {
			//var dname = clang.getCursorDisplayName(cursor).ToString();
			//if (!string.IsNullOrEmpty(dname) && dname.Contains("TemplMethodInTemplVar")) {
			//	var loc1 = new Location(clang.getCursorReferenced(cursor), Location.Kind.Expansion);
			//	var loc2 = new Location(clang.getCursorReferenced(cursor), Location.Kind.Presumed);
			//	var loc3 = new Location(clang.getCursorReferenced(cursor), Location.Kind.Spelling);
			//	var loc4 = new Location(clang.getCursorReferenced(cursor), Location.Kind.File);
			//}

			switch (cursor.kind) {
			case CXCursorKind.CXCursor_TypedefDecl:
				TypeOf(cursor);
				break;

			case CXCursorKind.CXCursor_StructDecl:
			case CXCursorKind.CXCursor_ClassDecl:
			case CXCursorKind.CXCursor_ClassTemplate:
				_ScopePath.Push(TypeOf(cursor));
				try {
					clang.visitChildren(cursor, this.VisitChild, new CXClientData());
				} finally {
					_ScopePath.Pop();
				}
				break;

			case CXCursorKind.CXCursor_FunctionDecl:
			case CXCursorKind.CXCursor_FunctionTemplate:
			case CXCursorKind.CXCursor_CXXMethod:
				_ScopePath.Push(FunctionOf(cursor));
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

			case CXCursorKind.CXCursor_Namespace:
				_ScopePath.Push(NamespaceOf(cursor));
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
					// マネージ状態を破棄します (マネージ オブジェクト)。
				}

				// アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// 大きなフィールドを null に設定します。
				foreach (var tu in _TranslationUnits.Values) {
					tu.Dispose();
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
