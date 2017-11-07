#pragma once
#include <string>
#include <memory>
#include <unordered_set>

template<class T>
std::size_t CalcHash(const T* nullTerminatedArray) {
#if defined(_WIN64)
	static_assert(sizeof(size_t) == 8, "This code is for 64-bit size_t.");
	const size_t _FNV_offset_basis = 14695981039346656037ULL;
	const size_t _FNV_prime = 1099511628211ULL;
#else /* defined(_WIN64) */
	static_assert(sizeof(size_t) == 4, "This code is for 32-bit size_t.");
	const size_t _FNV_offset_basis = 2166136261U;
	const size_t _FNV_prime = 16777619U;
#endif /* defined(_WIN64) */
	auto p = nullTerminatedArray;
	if (p) {
		auto _Val = _FNV_offset_basis;
		for (; *p; ++p) {
			_Val ^= size_t(*p);
			_Val *= _FNV_prime;
		}
		return (_Val);
	} else {
		return 0;
	}
}

class Location {
public:
	struct Hasher {
		typedef std::size_t result_type;
		std::size_t operator()(const Location& key) const noexcept {
			return CalcHash(key._FullName);
		}
	};

	struct Equals {
		bool operator()(const Location& left, const Location& right) const {	// apply operator== to operands
			if (left._FullName && right._FullName) {
				return strcmp(left._FullName, right._FullName) == 0;
			} else {
				return left._FullName == right._FullName;
			}
		}
	};

	Location() {
		_FullName = nullptr;
		_Owned = false;
	}
	Location(const Location& location) {
		if (location._Owned) {
			auto size = strlen(location._FullName) + 1;
			auto fullName = new char[size];
			memcpy(fullName, location._FullName, size);
			_FullName = fullName;
			_Owned = true;
		} else {
			_FullName = location._FullName;
			_Owned = false;
		}
	}
	Location(const char* fullName, bool owned = false) {
		_FullName = fullName;
		_Owned = owned;
	}
	~Location() {
		if (_FullName && _Owned) {
			delete _FullName;
		}
	}

	const char* FullName() const {
		return _FullName;
	}

private:
	const char* _FullName;
	bool _Owned;
};

class LocationManager {
public:
	LocationManager() {

	}

	std::unordered_set<std::unique_ptr<Location>, Location::Hasher, Location::Equals> _Locations;
};
