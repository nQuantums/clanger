#pragma once
#include <unordered_set>
#include "ConstString.h"


#pragma pack(push, 1)
struct Position {
	union {
		struct {
			int32_t Line;
			int32_t Column;
		};
		int64_t Value;
	};

	bool operator==(const Position& p) const {
		return this->Value == p.Value;
	}
	bool operator!=(const Position& p) const {
		return this->Value == p.Value;
	}
};

class Location : ConstString<char, Position> {
public:
private:
};

#pragma pack(pop)


struct PositionHasher {
	using argument_type = Position;
	using result_type = size_t;
	size_t operator()(const Position& _Keyval) const {
		return _Keyval.Value;
	}
};

struct PositionEquals {
	using argument_type = Position;
	using result_type = bool;
	bool operator()(const argument_type& _Left, const argument_type& _Right) const {
		return _Left == _Right;
	}
};

namespace std {
	struct hash<Position> : PositionHasher {};
	struct equal_to<Position> : PositionEquals {};

	template<class T, class Base> std::ostream& operator<<(std::ostream& os, const Position& value) {
		return os << "(" << value.Line << ", " << value.Column << ")";
	}
}




class FileBase {
public:
private:
	std::unordered_set<
};

class File : ConstString<char> {


};


//class LocationManager {
//public:
//	LocationManager() {
//	}
//
//	Location* Get(const char* path) {
//		Location refPath(path);
//		Location::UniquePtr upRef(&refPath);
//		auto iter = _Locations.find(upRef);
//		if (iter != _Locations.end()) {
//			return iter->get();
//		}
//
//		Location::UniquePtr up(Location::New(path));
//		_Locations.insert(up);
//		return up.get();
//	}
//
//private:
//	std::unordered_set<Location::UniquePtr> _Locations;
//};
