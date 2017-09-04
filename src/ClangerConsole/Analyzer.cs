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
		[DllImport("libclang", CallingConvention = CallingConvention.Cdecl, EntryPoint = "clang_disposeTokens")]
		private static extern void disposeTokens(CXTranslationUnit tu, IntPtr tokens, uint numTokens);
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
			public Dictionary<AbsLocation, Entity> Entities = new Dictionary<AbsLocation, Entity>();

			public File(string fullName) {
				this.FullName = fullName;
			}

			public override string ToString() {
				return this.FullName;
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

			public override string ToString() {
				return string.Concat(this.Line, ", ", this.Column);
			}
		}

		public struct Location {
			public File File;
			public Position Position;
			public string Name;
			public CXCursorKind Kind;

			public Location(File file, Position position, string name, CXCursorKind kind) {
				this.File = file;
				this.Position = position;
				this.Name = name;
				this.Kind = kind;
			}

			public override string ToString() {
				return string.Concat("<", this.File, ">(", this.Position, "): ", this.Name);
			}

			public override int GetHashCode() {
				return this.File.GetHashCode() ^ this.Position.GetHashCode() ^ this.Name.GetHashCode() ^ this.Kind.GetHashCode();
			}

			public override bool Equals(object obj) {
				if (obj is Location)
					return (Location)obj == this;
				return base.Equals(obj);
			}

			public static bool operator ==(Location a, Location b) {
				if (a.File != b.File)
					return false;
				if (a.Position != b.Position)
					return false;
				if (a.Name != b.Name)
					return false;
				if (a.Kind != b.Kind)
					return false;
				return true;
			}

			public static bool operator !=(Location a, Location b) {
				if (a.File != b.File)
					return true;
				if (a.Position != b.Position)
					return true;
				if (a.Name != b.Name)
					return true;
				if (a.Kind != b.Kind)
					return true;
				return false;
			}
		}

		public struct AbsLocation {
			public Location[] Path;

			public AbsLocation(Location[] path) {
				this.Path = path;
			}

			public override string ToString() {
				var sb = new StringBuilder();
				foreach(var p in this.Path) {
					sb.AppendLine(p.ToString());
				}
				return sb.ToString();
			}

			public override int GetHashCode() {
				int hc = 0;
				for (int i = 0; i < this.Path.Length; i++)
					hc ^= this.Path[i].GetHashCode();
				return hc;
			}

			public override bool Equals(object obj) {
				if (obj is AbsLocation)
					return (AbsLocation)obj == this;
				return base.Equals(obj);
			}

			public static bool operator ==(AbsLocation a, AbsLocation b) {
				if (a.Path.Length != b.Path.Length)
					return false;
				for (int i = 0; i < a.Path.Length; i++) {
					if (a.Path[i] != b.Path[i]) {
						return false;
					}
				}
				return true;
			}

			public static bool operator !=(AbsLocation a, AbsLocation b) {
				if (a.Path.Length != b.Path.Length)
					return true;
				for (int i = 0; i < a.Path.Length; i++) {
					if (a.Path[i] != b.Path[i]) {
						return true;
					}
				}
				return false;
			}
		}

		public class EntityBinder {
			Dictionary<string, File> _FilesDic = new Dictionary<string, File>();

			public EntityBinder() {
			}

			public File File(string fileName) {
				var fullName = System.IO.Path.GetFullPath(fileName).ToLower();
				File file;
				if (_FilesDic.TryGetValue(fullName, out file))
					return file;
				file = new File(fullName);
				_FilesDic[fullName] = file;
				return file;
			}

			public T Entity<T>(CXCursor cursor, Func<T> creator) where T : Entity {
				//CXString fileName;
				//Position position = new Position();
				//clang.getPresumedLocation(clang.getCursorLocation(cursor), out fileName, out position.Line, out position.Column);
				//var file = File(fileName.ToString());

				var c = cursor;
				uint offset1, offset2;
				var list = new List<Location>();
				File leafFile = null;
				string leafName = null;
				do {
					var loc = clang.getCursorLocation(c);

					CXFile f1, f2;
					Position position1 = new Position(), position2 = new Position();
					clang.getSpellingLocation(loc, out f1, out position1.Line, out position1.Column, out offset1);
					clang.getExpansionLocation(loc, out f2, out position2.Line, out position2.Column, out offset2);

					var file = File(clang.getFileName(f1).ToString());
					if (leafFile == null)
						leafFile = file;

					var name = clang.getCursorDisplayName(c).ToString();
					list.Add(new Location(file, position1, name, c.kind));
					if (leafName == null)
						leafName = name;

					c = clang.getCursorSemanticParent(c);
					
				} while (offset1 != offset2 && clang.isInvalid(c.kind) == 0 && c.kind != CXCursorKind.CXCursor_TranslationUnit);

				list.Reverse();

				var absLocation = new AbsLocation(list.ToArray());

				Entity e;
				if(leafFile.Entities.TryGetValue(absLocation, out e)) {
					var t = e as T;
					if (t == null)
						throw new ApplicationException(string.Concat("\"", e.FullName, "\" class mismatch: ", e.GetType().Name, " vs ", typeof(T).Name));
					return t;
				} else {
					if (creator == null)
						return null;
					var t = creator();
					t.Name = leafName;
					t.Cursors.Add(new CursorKey(cursor));
					t.AbsLocation = absLocation;
					leafFile.Entities.Add(absLocation, t);
					return t;
				}
			}
		}

		/// <summary>
		/// ソースコード上の何かに対応するエンティティ
		/// </summary>
		public class Entity {
			static readonly Entity InitialParent = new Entity(null);

			Entity _Parent = InitialParent;

			public string Name;
			public Analyzer Owner;
			public HashSet<CursorKey> Cursors = new HashSet<CursorKey>();
			public AbsLocation AbsLocation;
			public Dictionary<string, Entity> Children;

			public Entity Parent {
				get {
					if (_Parent == InitialParent) {
						var parentCursor = clang.getCursorSemanticParent(this.Cursors.First().Cursor);
						_Parent = clang.isInvalid(parentCursor.kind) == 0 && parentCursor.kind != CXCursorKind.CXCursor_TranslationUnit ? this.Owner.EntityOf<Entity>(parentCursor, null) : null;
					}
					return _Parent;
				}
			}
			public DecodedLocation ExpansionLocation => new DecodedLocation(this.Cursors.First().Cursor, DecodedLocation.Kind.Expansion);
			public DecodedLocation PresumedLocation => new DecodedLocation(this.Cursors.First().Cursor, DecodedLocation.Kind.Presumed);
			public DecodedLocation SpellingLocation => new DecodedLocation(this.Cursors.First().Cursor, DecodedLocation.Kind.Spelling);
			public DecodedLocation FileLocation => new DecodedLocation(this.Cursors.First().Cursor, DecodedLocation.Kind.File);
			public DecodedLocation InstantiateLocation => new DecodedLocation(this.Cursors.First().Cursor, DecodedLocation.Kind.Instantiate);

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

			public Entity[] Path {
				get {
					var path = new List<Entity>();
					var e = this;
					do {
						path.Add(e);
						e = e.Parent;
					} while (e != null);
					path.Reverse();
					return path.ToArray();
				}
			}

			public Entity(Analyzer owner) {
				this.Owner = owner;
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Namespace : Entity {
			public Namespace(Analyzer owner) : base(owner) {
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

			public Type(Analyzer owner) : base(owner) {
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

			public Variable(Analyzer owner) : base(owner) {
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Param : Entity {
			public CXType Type => clang.getCursorType(this.Cursors.First().Cursor);

			public Param(Analyzer owner) : base(owner) {
			}

			public override string ToString() {
				return this.FullName;
			}
		}

		public class Function : Entity {
			public CXType Type => clang.getCursorResultType(this.Cursors.First().Cursor);

			public Function(Analyzer owner) : base(owner) {
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
			public DecodedLocation Location;

			public Diagnostic(CXDiagnostic d) {
				this.Category = clang.getDiagnosticCategory(d);
				this.CategoryText = clang.getDiagnosticCategoryText(d).ToString();
				this.Text = clang.formatDiagnostic(d, unchecked((uint)-1)).ToString();
				this.Location = new DecodedLocation(clang.getDiagnosticLocation(d), DecodedLocation.Kind.Presumed);
				clang.disposeDiagnostic(d);
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

		public class DecodedLocation {
			public enum Kind {
				Expansion,
				Presumed,
				Spelling,
				File,
				Instantiate,
			}

			public string File;
			public string FullPath;
			public uint Line;
			public uint Column;
			public uint Offset;

			public DecodedLocation(CXSourceLocation loc, Kind kind = Kind.Presumed) {
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
				case Kind.Instantiate: {
						CXFile f;
						clang.getInstantiationLocation(loc, out f, out line, out column, out offset);
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

			public DecodedLocation(CXCursor c, Kind kind = Kind.Presumed)
				: this(clang.getCursorLocation(c), kind) {
			}

			public override string ToString() {
				return string.Concat("<\"", this.FullPath, "\">(", this.Line, ", ", this.Column, ", ", this.Offset, ")");
			}
		}

		public class Translation : IDisposable {
			public CXTranslationUnit TranslationUnit;
			public File SourceFile;

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
		Dictionary<CursorKey, Entity> _CursorToEntity = new Dictionary<CursorKey, Entity>();
		EntityBinder _EntityBinder = new EntityBinder();
		Dictionary<File, Translation> _TranslationUnits = new Dictionary<File, Translation>();
		Translation _CurrentTranslation;
		#endregion

		#region 公開メソッド
		public Analyzer() {
			_Index = clang.createIndex(0, 0);
		}

		public void Parse(string sourceFile, string[] includeDirs = null, string[] additionalOptions = null, bool msCompati = true) {
			var file = _EntityBinder.File(sourceFile);
			if (_TranslationUnits.ContainsKey(file))
				return;

			var ufile = new CXUnsavedFile();
			var tu = new CXTranslationUnit();
			var options = new List<string>(new string[] { "-std=c++14", "-ferror-limit=9999" });

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
			foreach(var d in diags) {
				Console.WriteLine(d);
			}

			// 本当はここでエラーチェックが好ましい
			var cursor = clang.getTranslationUnitCursor(tu);

			_CurrentTranslation = new Translation(tu, file);

			clang.visitChildren(cursor, this.VisitChild, new CXClientData());


			//foreach(var e in new List<Entity>(_CursorToEntity.Values)) {
			//	Console.WriteLine(e.FullName);
			//}

			_TranslationUnits[file] = _CurrentTranslation;
		}
		#endregion

		#region 内部メソッド
		static CXToken[] Tokenize(CXTranslationUnit tu, CXSourceRange range) {
			IntPtr pTokens;
			uint numTokens;
			clang.tokenize(tu, range, out pTokens, out numTokens);
			try {
				var tokens = new CXToken[numTokens];
				unsafe {
					var p = (CXToken*)pTokens.ToPointer();
					for (uint i = 0; i < numTokens; i++) {
						tokens[i] = p[i];
					}
				}
				return tokens;
			} finally {
				disposeTokens(tu, pTokens, numTokens);
			}
		}

		T EntityOf<T>(CXCursor cursor, Func<T> creator) where T : Entity {
			Entity e;
			var key = new CursorKey(cursor);
			if (_CursorToEntity.TryGetValue(key, out e)) {
				var t = e as T;
				if (t == null)
					throw new ApplicationException(string.Concat("\"", e.FullName, "\" class mismatch: ", e.GetType().Name, " vs ", typeof(T).Name));
				return t;
			} else {
				var t = _EntityBinder.Entity<T>(cursor, creator);
				if (creator == null)
					return t;
				_CursorToEntity.Add(key, t);
				return t;
			}
		}

		Namespace NamespaceOf(CXCursor cursor) {
			return EntityOf<Namespace>(cursor, () => new Namespace(this));
		}

		Type TypeOf(CXCursor cursor) {
			return EntityOf<Type>(cursor, () => new Type(this));
		}

		Variable VariableOf(CXCursor cursor) {
			return EntityOf<Variable>(cursor, () => new Variable(this));
		}

		Function FunctionOf(CXCursor cursor) {
			return EntityOf<Function>(cursor, () => new Function(this));
		}

		Param ParamOf(CXCursor cursor) {
			return EntityOf<Param>(cursor, () => new Param(this));
		}

		static string FullName(CXCursor cursor) {
			if (clang.isInvalid(cursor.kind) != 0 || cursor.kind == CXCursorKind.CXCursor_TranslationUnit)
				return "";
			var path = FullName(clang.getCursorSemanticParent(cursor));
			var name = clang.getCursorDisplayName(cursor).ToString();
			if (path.Length != 0)
				return string.Concat(path, NameScopeDelimiter, name);
			return name;
			//return string.Concat(path, new DecodedLocation(cursor), "\n", name, "\n");
		}

		CXChildVisitResult VisitChild(CXCursor cursor, CXCursor parent, IntPtr client_data) {
			//var dname = clang.getCursorDisplayName(cursor).ToString();
			//if (!string.IsNullOrEmpty(dname) && dname == "_InterlockedIncrement" /*dname.Contains("_InterlockedIncrement")*/) {
			//	var loc1 = new DecodedLocation(clang.getCursorReferenced(cursor), DecodedLocation.Kind.Spelling);
			//}

			switch (cursor.kind) {
			case CXCursorKind.CXCursor_TypedefDecl:
			case CXCursorKind.CXCursor_StructDecl:
			case CXCursorKind.CXCursor_ClassDecl:
			case CXCursorKind.CXCursor_ClassTemplate:
			case CXCursorKind.CXCursor_ClassTemplatePartialSpecialization:
				TypeOf(cursor);
				break;

			case CXCursorKind.CXCursor_FunctionDecl:
			case CXCursorKind.CXCursor_FunctionTemplate:
			case CXCursorKind.CXCursor_CXXMethod:
				FunctionOf(cursor);
				break;

			case CXCursorKind.CXCursor_VarDecl:
			case CXCursorKind.CXCursor_FieldDecl:
				VariableOf(cursor);
				break;

			case CXCursorKind.CXCursor_ParmDecl:
				ParamOf(cursor);
				break;

			case CXCursorKind.CXCursor_Namespace:
				NamespaceOf(cursor);
				break;

			case CXCursorKind.CXCursor_CallExpr: {
					var cursorDef = clang.getCursorDefinition(cursor);
					if (clang.isInvalid(cursorDef.kind) != 0 || !IsFunction(cursorDef.kind)) {
						cursorDef = clang.getCursorReferenced(cursor);
					}

					if (clang.isInvalid(cursorDef.kind) == 0 && IsFunction(cursorDef.kind)) {
						var loc = new DecodedLocation(cursor, DecodedLocation.Kind.Spelling);
						var func = FunctionOf(cursorDef);
						Console.WriteLine(string.Concat(loc, " : ", func.FullName));
					}
				}
				break;

			case CXCursorKind.CXCursor_BinaryOperator:
				break;

			default:
				break;
			}

			return CXChildVisitResult.CXChildVisit_Recurse;
		}

		static bool IsFunction(CXCursorKind kind) {
			switch(kind) {
			case CXCursorKind.CXCursor_FunctionDecl:
			case CXCursorKind.CXCursor_FunctionTemplate:
			case CXCursorKind.CXCursor_CXXMethod:
				return true;
			default:
				return false;
			}
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
