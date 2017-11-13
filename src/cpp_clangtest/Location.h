#pragma once
#include <unordered_set>
#include "ConstString.h"


using Location = ConstStringA;

struct Position {
	int Line;
	int Column;
};


class File : ConstString<char, File> {


};


class LocationManager {
public:
	LocationManager() {
	}

	Location* Get(const char* path) {
		Location refPath(path);
		Location::UniquePtr upRef(&refPath);
		auto iter = _Locations.find(upRef);
		if (iter != _Locations.end()) {
			return iter->get();
		}

		Location::UniquePtr up(Location::New(path));
		_Locations.insert(up);
		return up.get();
	}

private:
	std::unordered_set<Location::UniquePtr> _Locations;
};
