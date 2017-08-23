//===- CXCursor.cpp - Routines for manipulating CXCursors -----------------===//
//
//                     The LLVM Compiler Infrastructure
//
// This file is distributed under the University of Illinois Open Source
// License. See LICENSE.TXT for details.
//
//===----------------------------------------------------------------------===//
//
// This file defines routines for manipulating CXCursors. It should be the
// only file that has internal knowledge of the encoding of the data in
// CXCursor.
//
//===----------------------------------------------------------------------===//

#include "CXTranslationUnit.h"
#include "CXCursor.h"
#include "CXString.h"
#include "CXType.h"
#include "clang-c/Index.h"
#include "clang/AST/Attr.h"
#include "clang/AST/Decl.h"
#include "clang/AST/DeclCXX.h"
#include "clang/AST/DeclObjC.h"
#include "clang/AST/DeclTemplate.h"
#include "clang/AST/Expr.h"
#include "clang/AST/ExprCXX.h"
#include "clang/AST/ExprObjC.h"
#include "clang/Frontend/ASTUnit.h"
#include "llvm/Support/ErrorHandling.h"

using namespace clang;
using namespace cxcursor;

/************************************************************************
* Duplicated libclang functionality
*
* The following methods are duplicates of methods that are implemented
* in libclang, but aren't exposed as symbols that can be used by third-
* party libraries.
************************************************************************/

//namespace clang {
//	enum CXStringFlag {
//		/// CXString contains a 'const char *' that it doesn't own.
//		CXS_Unmanaged,
//
//		/// CXString contains a 'const char *' that it allocated with malloc().
//		CXS_Malloc,
//
//		/// CXString contains a CXStringBuf that needs to be returned to the
//		/// CXStringPool.
//		CXS_StringBuf
//	};
//
//	const clang::Stmt *getCursorStmt(CXCursor cursor) {
//		if (cursor.kind == CXCursor_ObjCSuperClassRef ||
//			cursor.kind == CXCursor_ObjCProtocolRef ||
//			cursor.kind == CXCursor_ObjCClassRef) {
//
//			return nullptr;
//		}
//		return static_cast<const clang::Stmt *>(cursor.data[1]);
//	}
//
//	const clang::Expr *getCursorExpr(CXCursor cursor) {
//		return clang::dyn_cast_or_null<clang::Expr>(getCursorStmt(cursor));
//	}
//
//	namespace cxstring {
//		CXString createEmpty() {
//			CXString str;
//			str.data = "";
//			str.private_flags = CXS_Unmanaged;
//			return str;
//		}
//
//		CXString createDup(StringRef string) {
//			CXString result;
//			char *spelling = static_cast<char *>(malloc(string.size() + 1));
//			memmove(spelling, string.data(), string.size());
//			spelling[string.size()] = 0;
//			result.data = spelling;
//			result.private_flags = (unsigned)CXS_Malloc;
//			return result;
//		}
//
//	}
//}

/************************************************************************
* New Sealang functionality
*
* The following methods expose useful features of the LLVM AST. They are
* all potentially candidates for inclusion upstream in libclang.
************************************************************************/

CXString clang_Cursor_getOperatorString(CXCursor cursor)
{
	if (cursor.kind == CXCursor_BinaryOperator) {
		clang::BinaryOperator *op = (clang::BinaryOperator *) cxcursor::getCursorExpr(cursor);
		return clang::cxstring::createDup(clang::BinaryOperator::getOpcodeStr(op->getOpcode()));
	}

	if (cursor.kind == CXCursor_CompoundAssignOperator) {
		clang::CompoundAssignOperator *op = (clang::CompoundAssignOperator*) cxcursor::getCursorExpr(cursor);
		return clang::cxstring::createDup(clang::BinaryOperator::getOpcodeStr(op->getOpcode()));
	}

	if (cursor.kind == CXCursor_UnaryOperator) {
		clang::UnaryOperator *op = (clang::UnaryOperator*) cxcursor::getCursorExpr(cursor);
		return clang::cxstring::createDup(clang::UnaryOperator::getOpcodeStr(op->getOpcode()));
	}

	return clang::cxstring::createEmpty();
}

int clang_Cursor_getBinaryOpcode(CXCursor cursor)
{
	if (cursor.kind == CXCursor_BinaryOperator) {
		clang::BinaryOperator *op = (clang::BinaryOperator *) cxcursor::getCursorExpr(cursor);
		return static_cast<clang::BinaryOperatorKind>(op->getOpcode());
	}

	if (cursor.kind == CXCursor_CompoundAssignOperator) {
		clang::CompoundAssignOperator *op = (clang::CompoundAssignOperator *) cxcursor::getCursorExpr(cursor);
		return static_cast<clang::BinaryOperatorKind>(op->getOpcode());
	}

	return (clang::BinaryOperatorKind) 99999;
}

int clang_Cursor_getUnaryOpcode(CXCursor cursor)
{
	if (cursor.kind == CXCursor_UnaryOperator) {
		clang::UnaryOperator *op = (clang::UnaryOperator*) cxcursor::getCursorExpr(cursor);
		return static_cast<clang::UnaryOperatorKind>(op->getOpcode());
	}

	return (clang::UnaryOperatorKind) 99999;
}

CXString clang_Cursor_getLiteralString(CXCursor cursor)
{
	if (cursor.kind == CXCursor_IntegerLiteral) {
		clang::IntegerLiteral *intLiteral = (clang::IntegerLiteral *) cxcursor::getCursorExpr(cursor);
		return clang::cxstring::createDup(intLiteral->getValue().toString(10, true));
	}

	if (cursor.kind == CXCursor_FloatingLiteral) {
		clang::FloatingLiteral *floatLiteral = (clang::FloatingLiteral *) cxcursor::getCursorExpr(cursor);
		llvm::SmallString<1024> str;
		floatLiteral->getValue().toString(str);
		return clang::cxstring::createDup(str.c_str());
	}

	if (cursor.kind == CXCursor_CharacterLiteral) {
		clang::CharacterLiteral *charLiteral = (clang::CharacterLiteral *) cxcursor::getCursorExpr(cursor);
		char c[2];
		c[0] = (char)charLiteral->getValue();
		c[1] = '\0';
		return clang::cxstring::createDup(c);
	}

	if (cursor.kind == CXCursor_StringLiteral) {
		clang::StringLiteral *stringLiteral = (clang::StringLiteral *) cxcursor::getCursorExpr(cursor);
		return clang::cxstring::createDup(stringLiteral->getBytes());
	}

	if (cursor.kind == CXCursor_CXXBoolLiteralExpr) {
		clang::CXXBoolLiteralExpr *boolLiteral = (clang::CXXBoolLiteralExpr *) cxcursor::getCursorExpr(cursor);
		return clang::cxstring::createDup(boolLiteral->getValue() ? "true" : "false");
	}

	return clang::cxstring::createEmpty();
}
