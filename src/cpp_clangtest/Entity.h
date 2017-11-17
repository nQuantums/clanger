#pragma once
#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <string>
#include <memory>

#include "ClangCommon.h"
#include "ConstString.h"
#include "FileManager.h"


class Entity {
public:
#define ENTITY_KIND(x, isDecl) x,
	enum class Kinds {
		Unknown = 0,
#include "EntityKinds.inc"
	};
#undef ENTITY_KIND

#define ENTITY_KIND(x, isDecl)\
	Entity(clang::x* p, Location* loc) {\
		this->Kind = Kinds::x;\
		this->Node = p;\
		if (isDecl) {\
			SetName(reinterpret_cast<clang::Decl*>(p));\
			SetUsr(reinterpret_cast<clang::Decl*>(p));\
		}\
		this->Loc = loc;\
	}
#include "EntityKinds.inc"
#undef ENTITY_KIND
	~Entity() {
	}

	Kinds Kind;
	ConstStringA::UniquePtr Name;
	ConstStringA::UniquePtr Usr;
	Location* Loc;
	std::vector<Entity*> Children;

#define ENTITY_KIND(x, isDecl) case Kinds::x: return isDecl;
	bool IsDecl() const {
		switch (this->Kind) {
#include "EntityKinds.inc"
		default: return false;
		}
	}
#undef ENTITY_KIND

private:
	void SetName(clang::Decl* D) {
		if (clang::NamedDecl* N = clang::dyn_cast<clang::NamedDecl>(D)) {
			this->Name = ConstStringA::New(N->getNameAsString().c_str());
		}
	}

	void SetUsr(clang::Decl* D) {
		llvm::SmallString<512> buf;
		clang::index::generateUSRForDecl(D, buf);
		buf.push_back('\0');
		auto size = buf.size();
		auto p = new char[size];
		memcpy(p, buf.data(), size);
		this->Usr = ConstStringA::New(p);
	}
};
