#pragma once
#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <string>
#include <memory>

#include "ClangCommon.h"


class Entity {
public:
#define ENTITY_KIND(x, isDecl) x,
	enum class Kinds {
		Unknown = 0,
#include "EntityKinds.inc"
	};
#undef ENTITY_KIND

#define ENTITY_KIND(x, isDecl) Entity(clang::x* p) { this->Kind = Kinds::x; this->Node = p; if (isDecl) { SetUsr(reinterpret_cast<clang::Decl*>(p)); } else { this->Usr = nullptr; } }
#include "EntityKinds.inc"
#undef ENTITY_KIND
	~Entity() {
		if (this->Usr)
			delete this->Usr;
	}

#define ENTITY_KIND(x, isDecl) clang::FunctionDecl* x;
	union {
		void* Node;
#include "EntityKinds.inc"
	};
#undef ENTITY_KIND
	Kinds Kind;
	const char* Usr;
	std::vector<Entity*> Referers;

#define ENTITY_KIND(x, isDecl) case Kinds::x: return isDecl;
	bool IsDecl() const {
		switch (this->Kind) {
#include "EntityKinds.inc"
		default: return false;
		}
	}
#undef ENTITY_KIND
	clang::Decl* Decl() const {
		return IsDecl() ? reinterpret_cast<clang::Decl*>(this->Node) : nullptr;
	}
	clang::Stmt* Stmt() const {
		return IsDecl() ? nullptr : reinterpret_cast<clang::Stmt*>(this->Node);
	}
	std::string Name() const {
		auto decl = Decl();
		if (clang::NamedDecl* N = clang::dyn_cast<clang::NamedDecl>(decl)) {
			return N->getNameAsString();
		} else {
			return "";
		}
	}

private:
	void SetUsr(clang::Decl* D) {
		llvm::SmallString<512> buf;
		clang::index::generateUSRForDecl(D, buf);
		buf.push_back('\0');
		auto size = buf.size();
		auto p = new char[size];
		memcpy(p, buf.data(), size);
		this->Usr = p;
	}
};
