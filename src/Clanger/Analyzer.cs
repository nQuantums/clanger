using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClangSharp;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Clanger {
	public class Analyzer : IDisposable {
		const string NameScopeDelimiter = ".";

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
			public Dictionary<LocationPath, Entity> Entities = new Dictionary<LocationPath, Entity>();

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

		public struct LocationItem {
			public File File;
			public Position Position;
			public string Name;
			public CXCursorKind Kind;

			public LocationItem(File file, Position position, string name, CXCursorKind kind) {
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
				if (obj is LocationItem)
					return (LocationItem)obj == this;
				return base.Equals(obj);
			}

			public static bool operator ==(LocationItem a, LocationItem b) {
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

			public static bool operator !=(LocationItem a, LocationItem b) {
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

		public struct LocationPath {
			public LocationItem[] Path;

			public LocationPath(LocationItem[] path) {
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
				if (obj is LocationPath)
					return (LocationPath)obj == this;
				return base.Equals(obj);
			}

			public static bool operator ==(LocationPath a, LocationPath b) {
				if (a.Path.Length != b.Path.Length)
					return false;
				for (int i = 0; i < a.Path.Length; i++) {
					if (a.Path[i] != b.Path[i]) {
						return false;
					}
				}
				return true;
			}

			public static bool operator !=(LocationPath a, LocationPath b) {
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
				var c = cursor;
				uint offset1, offset2;
				var list = new List<LocationItem>();
				File leafFile = null;
				string leafName = null;
				do {
					var loc = clang.getCursorLocation(c);

					CXFile f1, f2;
					Position position1 = new Position(), position2 = new Position();
					clang.getSpellingLocation(loc, out f1, out position1.Line, out position1.Column, out offset1);
					if (position1.Line == 0)
						return null;
					clang.getExpansionLocation(loc, out f2, out position2.Line, out position2.Column, out offset2);

					var file = File(clang.getFileName(f1).ToString());
					if (leafFile == null)
						leafFile = file;

					var name = clang.getCursorDisplayName(c).ToString();
					list.Add(new LocationItem(file, position1, name, c.kind));
					if (leafName == null)
						leafName = name;

					c = clang.getCursorSemanticParent(c);
					
				} while (offset1 != offset2 && clang.isInvalid(c.kind) == 0 && c.kind != CXCursorKind.CXCursor_TranslationUnit);

				list.Reverse();

				var locationPath = new LocationPath(list.ToArray());

				Entity e;
				if(leafFile.Entities.TryGetValue(locationPath, out e)) {
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
					t.LocationPath = locationPath;
					leafFile.Entities.Add(locationPath, t);
					return t;
				}
			}
		}

		public class LightEntity {
			public CXCursor Cursor;
			public int Depth;

			public DecodedLocation ExpansionLocation => new DecodedLocation(this.Cursor, DecodedLocation.Kind.Expansion);
			public DecodedLocation PresumedLocation => new DecodedLocation(this.Cursor, DecodedLocation.Kind.Presumed);
			public DecodedLocation SpellingLocation => new DecodedLocation(this.Cursor, DecodedLocation.Kind.Spelling);
			public DecodedLocation FileLocation => new DecodedLocation(this.Cursor, DecodedLocation.Kind.File);
			public DecodedLocation InstantiateLocation => new DecodedLocation(this.Cursor, DecodedLocation.Kind.Instantiate);

			public string Name {
				get {
					return clang.getCursorDisplayName(this.Cursor).ToString();
				}
			}

			public string DisplayName {
				get {
					var kindStr = this.Cursor.kind.ToString();
					if (kindStr.StartsWith("CXCursor_"))
						kindStr = kindStr.Substring(9);
					//var name = FullName(this.Cursor); // this.Name;
					var name = clang.getCursorUSR(this.Cursor).ToString(); // this.Name;
					switch (this.Cursor.kind) {
					case CXCursorKind.CXCursor_UnaryOperator:
					case CXCursorKind.CXCursor_BinaryOperator:
					case CXCursorKind.CXCursor_CompoundAssignOperator:
					case CXCursorKind.CXCursor_ConditionalOperator:
						name = clang.Cursor_getOperatorString(this.Cursor).ToString();
						break;
					case CXCursorKind.CXCursor_CharacterLiteral:
					case CXCursorKind.CXCursor_StringLiteral:
					case CXCursorKind.CXCursor_FloatingLiteral:
					case CXCursorKind.CXCursor_IntegerLiteral:
						name = clang.Cursor_getLiteralString(this.Cursor).ToString();
						break;
					}
					var flags = new StringBuilder();
					if (this.IsDeclaration) { if (flags.Length != 0) flags.Append(", "); flags.Append("decl"); }
					if (this.IsDefinition) { if (flags.Length != 0) flags.Append(", "); flags.Append("def"); }
					if (this.IsReference) { if (flags.Length != 0) flags.Append(", "); flags.Append("ref"); }
					if (this.IsAttribute) { if (flags.Length != 0) flags.Append(", "); flags.Append("atr"); }
					if (this.IsAnonymous) { if (flags.Length != 0) flags.Append(", "); flags.Append("ano"); }
					if (this.IsBitField) { if (flags.Length != 0) flags.Append(", "); flags.Append("bit"); }
					if (this.IsDynamicCall) { if (flags.Length != 0) flags.Append(", "); flags.Append("dyn"); }
					if (this.IsVariadic) { if (flags.Length != 0) flags.Append(", "); flags.Append("vari"); }
					if (this.IsMutable) { if (flags.Length != 0) flags.Append(", "); flags.Append("mut"); }
					if (this.IsConst) { if (flags.Length != 0) flags.Append(", "); flags.Append("const"); }
					if (this.IsVirtualBase) { if (flags.Length != 0) flags.Append(", "); flags.Append("virtual base"); }
					if (this.IsVirtual) { if (flags.Length != 0) flags.Append(", "); flags.Append("virtual"); }
					if (this.IsPureVirtual) { if (flags.Length != 0) flags.Append(", "); flags.Append("pure virtual"); }
					if (this.IsStatic) { if (flags.Length != 0) flags.Append(", "); flags.Append("static"); }
					if (this.IsPreprocessing) { if (flags.Length != 0) flags.Append(", "); flags.Append("prepro"); }
					if (this.IsStatement) { if (flags.Length != 0) flags.Append(", "); flags.Append("statement"); }
					if (this.IsTranslationUnit) { if (flags.Length != 0) flags.Append(", "); flags.Append("tu"); }
					if (this.IsUnexposed) { if (flags.Length != 0) flags.Append(", "); flags.Append("unex"); }
					if (this.IsExpression) { if (flags.Length != 0) flags.Append(", "); flags.Append("expr"); }
					return string.Concat(new string(' ', this.Depth * 4), kindStr, ": ", name, flags.Length != 0 ? string.Concat(" [", flags.ToString(), "]") : "");
				}
			}

			public bool IsDeclaration => clang.isDeclaration(this.Cursor.kind) != 0;
			public bool IsDefinition => clang.isCursorDefinition(this.Cursor) != 0;
			public bool IsReference => clang.isReference(this.Cursor.kind) != 0;
			public bool IsAttribute => clang.isAttribute(this.Cursor.kind) != 0;
			public bool IsAnonymous => clang.Cursor_isAnonymous(this.Cursor) != 0;
			public bool IsBitField => clang.Cursor_isBitField(this.Cursor) != 0;
			public bool IsDynamicCall => clang.Cursor_isDynamicCall(this.Cursor) != 0;
			public bool IsVariadic => clang.Cursor_isVariadic(this.Cursor) != 0;
			public bool IsMutable => clang.CXXField_isMutable(this.Cursor) != 0;
			public bool IsConst => clang.CXXMethod_isConst(this.Cursor) != 0;
			public bool IsPureVirtual => clang.CXXMethod_isPureVirtual(this.Cursor) != 0;
			public bool IsStatic =>clang.CXXMethod_isStatic(this.Cursor) != 0;
			public bool IsVirtual => clang.CXXMethod_isVirtual(this.Cursor) != 0;
			public bool IsPreprocessing => clang.isPreprocessing(this.Cursor.kind) != 0;
			public bool IsStatement => clang.isStatement(this.Cursor.kind) != 0;
			public bool IsTranslationUnit => clang.isTranslationUnit(this.Cursor.kind) != 0;
			public bool IsUnexposed => clang.isUnexposed(this.Cursor.kind) != 0;
			public bool IsVirtualBase => clang.isVirtualBase(this.Cursor) != 0;
			public bool IsExpression => clang.isExpression(this.Cursor.kind) != 0;

			public CXCursor DefinitionCursor => clang.getCursorDefinition(this.Cursor);
			public CXCursor ReferencedCursor => clang.getCursorReferenced(this.Cursor);
			public CXCursor CanonicalCursor => clang.getCanonicalCursor(this.Cursor);
			public CXCursor SemanticParentCursor => clang.getCursorSemanticParent(this.Cursor);
			public CXCursor LexicalParentCursor => clang.getCursorLexicalParent(this.Cursor);
			public CXCursor SpecializedTemplateCursor => clang.getSpecializedCursorTemplate(this.Cursor);

			public Tuple<IntPtr, IntPtr, uint, uint, uint, uint> DefinitionSpellingAndExtent {
				get {
					IntPtr startBuf;
					IntPtr endBuf;
					uint startLine;
					uint startColumn;
					uint endLine;
					uint endColumn;
					clang.getDefinitionSpellingAndExtent(this.Cursor, out startBuf, out endBuf, out startLine, out startColumn, out endLine, out endColumn);
					return new Tuple<IntPtr, IntPtr, uint, uint, uint, uint>(startBuf, endBuf, startLine, startColumn, endLine, endColumn);
				}
			}

			public LightEntity(CXCursor cursor, int depth) {
				this.Cursor = cursor;
				this.Depth = depth;
			}
		}

		/// <summary>
		/// ソースコード上の何かに対応するエンティティ
		/// </summary>
		public class Entity {
			static readonly Entity InitialParent = new Entity(null);

			Entity _Parent = InitialParent;

			public string Name;
			public int Depth;
			public CXCursorKind Kind;
			public Analyzer Owner;
			public HashSet<CursorKey> Cursors = new HashSet<CursorKey>();
			public LocationPath LocationPath;
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

			public string DisplayName {
				get {
					return string.Concat(new string(' ', this.Depth), this.Kind.ToString().Substring(9), " : ", this.Name);
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
		}

		public class Variable : Entity {
			public CXType Type => clang.getCursorType(this.Cursors.First().Cursor);

			public Variable(Analyzer owner) : base(owner) {
			}
		}

		public class Param : Entity {
			public CXType Type => clang.getCursorType(this.Cursors.First().Cursor);

			public Param(Analyzer owner) : base(owner) {
			}
		}

		public class Function : Entity {
			public CXType Type => clang.getCursorResultType(this.Cursors.First().Cursor);

			public Function(Analyzer owner) : base(owner) {
			}
		}

		public class FunctionCall : Entity {
			public Function Function {
				get {
					var cursor = this.Cursors.First().Cursor;
					var cursorDef = clang.getCursorDefinition(cursor);
					if (clang.isInvalid(cursorDef.kind) != 0 || !IsFunction(cursorDef.kind)) {
						cursorDef = clang.getCursorReferenced(cursor);
					}

					if (clang.isInvalid(cursorDef.kind) == 0 && IsFunction(cursorDef.kind)) {
						return this.Owner.EntityOf<Function>(cursorDef, () => new Function(this.Owner));
					} else {
						Console.WriteLine(string.Concat(this.SpellingLocation, ": ", this.Name));
						return null;
					}
				}
			}

			public FunctionCall(Analyzer owner) : base(owner) {
			}
		}

		public class VariableRef : Entity {
			public Variable Variable {
				get {
					var cursor = this.Cursors.First().Cursor;
					var cursorDef = clang.getCursorDefinition(cursor);
					if (clang.isInvalid(cursorDef.kind) != 0 || !IsVariable(cursorDef.kind)) {
						cursorDef = clang.getCursorReferenced(cursor);
					}

					if (clang.isInvalid(cursorDef.kind) == 0 && IsVariable(cursorDef.kind)) {
						return this.Owner.EntityOf<Variable>(cursorDef, () => new Variable(this.Owner));
					} else {
						return null;
					}
				}
			}

			public VariableRef(Analyzer owner) : base(owner) {
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
				//clang.findReferencesInFile
				//clang.index_getClientEntity()

				//clang.getFileUniqueID
				//clang.getLocation(tu, )
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
		HashSet<CXCursorKind> _UsedKinds = new HashSet<CXCursorKind>();
		public List<LightEntity> Entities = new List<LightEntity>(); // TODO: 削除する
		int _Depth = 1;
		Dictionary<string, CXCursor> _UsrCursors = new Dictionary<string, CXCursor>();
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

			// TODO: 削除する
			var kinds = new List<CXCursorKind>(_UsedKinds);
			kinds.Sort();
			foreach(var kind in kinds) {
				Console.WriteLine(kind);
			}

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
				clang.disposeTokens(tu, pTokens, numTokens);
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
				if (t == null)
					return null;
				t.Depth = _Depth;
				_CursorToEntity.Add(key, t);
				return t;
			}
		}

		Entity EntityOf(CXCursor cursor) {
			switch (cursor.kind) {
			case CXCursorKind.CXCursor_TypedefDecl:
			case CXCursorKind.CXCursor_StructDecl:
			case CXCursorKind.CXCursor_UnionDecl:
			case CXCursorKind.CXCursor_EnumDecl:
			case CXCursorKind.CXCursor_ClassDecl:
			case CXCursorKind.CXCursor_ClassTemplate:
			case CXCursorKind.CXCursor_ClassTemplatePartialSpecialization:
				// 型など宣言
				return EntityOf<Type>(cursor, () => new Type(this));

			case CXCursorKind.CXCursor_FunctionDecl:
			case CXCursorKind.CXCursor_FunctionTemplate:
			case CXCursorKind.CXCursor_Constructor:
			case CXCursorKind.CXCursor_Destructor:
			case CXCursorKind.CXCursor_CXXMethod:
			case CXCursorKind.CXCursor_ConversionFunction:
				// 関数、メソッドなど宣言
				return EntityOf<Function>(cursor, () => new Function(this));

			case CXCursorKind.CXCursor_VarDecl:
			case CXCursorKind.CXCursor_FieldDecl:
				// 変数など宣言
				return EntityOf<Variable>(cursor, () => new Variable(this));

			case CXCursorKind.CXCursor_ParmDecl:
				// 引数宣言
				return EntityOf<Param>(cursor, () => new Param(this));

			case CXCursorKind.CXCursor_Namespace:
				// ネームスペース
				return EntityOf<Namespace>(cursor, () => new Namespace(this));

			case CXCursorKind.CXCursor_CallExpr:
				// 関数呼び出し
				return EntityOf<FunctionCall>(cursor, () => new FunctionCall(this));

			case CXCursorKind.CXCursor_DeclRefExpr:
			case CXCursorKind.CXCursor_MemberRef:
			case CXCursorKind.CXCursor_MemberRefExpr:
				// 参照
				return EntityOf<VariableRef>(cursor, () => new VariableRef(this));

			//case CXCursorKind.CXCursor_BinaryOperator:
			//	break;


			// TODO: ↓の種類を処理する必要がありそう
			//case CXCursorKind.CXCursor_EnumConstantDecl:
			//	break;
			//case CXCursorKind.CXCursor_NullStmt:
			//	break;
			//case CXCursorKind.CXCursor_MemberRefExpr:
			//	//ShowCode(cursor);
			//	break;
			//case CXCursorKind.CXCursor_PackExpansionExpr:
			//	//ShowCode(cursor);
			//	break;
			//case CXCursorKind.CXCursor_UnaryExpr:
			//	//ShowCode(cursor);
			//	break;
			//case CXCursorKind.CXCursor_TemplateTypeParameter:
			//	ShowCode(cursor);
			//	break;
			//case CXCursorKind.CXCursor_NonTypeTemplateParameter:
			//	ShowCode(cursor);
			//	break;
			//case CXCursorKind.CXCursor_TemplateTemplateParameter:
			//	ShowCode(cursor);
			//	break;

			default:
				return null;
			}
		}

		static string FullName(CXCursor cursor) {
			if (clang.isInvalid(cursor.kind) != 0 || cursor.kind == CXCursorKind.CXCursor_TranslationUnit)
				return "";
			var path = FullName(clang.getCursorSemanticParent(cursor));
			var name = clang.getCursorDisplayName(cursor).ToString();
			if (path.Length != 0)
				return string.Concat(path, NameScopeDelimiter, name);
			return name;
		}

		static void ShowCode(CXCursor cursor) {
			var loc = new DecodedLocation(cursor, DecodedLocation.Kind.Spelling);
			var p = Process.Start("code", string.Concat("-g \"", loc.FullPath, "\":", loc.Line, "\":", loc.Column));
			p.WaitForExit();
		}

		CXChildVisitResult VisitChild(CXCursor cursor, CXCursor parent, IntPtr client_data) {
			//var dname = clang.getCursorDisplayName(cursor).ToString();
			//if (!string.IsNullOrEmpty(dname) && dname == "_InterlockedIncrement" /*dname.Contains("_InterlockedIncrement")*/) {
			//	var loc1 = new DecodedLocation(clang.getCursorReferenced(cursor), DecodedLocation.Kind.Spelling);
			//}

			//_UsedKinds.Add(cursor.kind);

			//_Kind = cursor.kind;
			//var entity = EntityOf(cursor);
			var usr = clang.getCursorUSR(cursor).ToString();
			if (usr.Length != 0)
				_UsrCursors[usr] = cursor;

			this.Entities.Add(new LightEntity(cursor, _Depth));

			_Depth++;
			clang.visitChildren(cursor, new CXCursorVisitor(this.VisitChild), new CXClientData());
			_Depth--;


			//return CXChildVisitResult.CXChildVisit_Recurse;

			// 次の要素へ移動
			return CXChildVisitResult.CXChildVisit_Continue;
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

		static bool IsVariable(CXCursorKind kind) {
			switch (kind) {
			case CXCursorKind.CXCursor_VarDecl:
			case CXCursorKind.CXCursor_FieldDecl:
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
