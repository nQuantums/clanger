/**
* \brief Returns string representation of unary and binary operators
*/
CINDEX_LINKAGE CXString clang_Cursor_getOperatorString(CXCursor cursor);

/**
* \brief Returns Opcode of binary operator
*/
CINDEX_LINKAGE int clang_Cursor_getBinaryOpcode(CXCursor cursor);

/**
* \brief Returns Opcode of unary operator
*/
CINDEX_LINKAGE int clang_Cursor_getUnaryOpcode(CXCursor cursor);

/**
* \brief Returns string representation of literal cursor (1.f, 1000L, etc)
*/
CINDEX_LINKAGE CXString clang_Cursor_getLiteralString(CXCursor cursor);
