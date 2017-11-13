//#pragma intrinsic(_InterlockedIncrement)
//extern "C" long _InterlockedIncrement(long volatile *);

#include <Windows.h>
//#include <vector>
//#include "../ClangerConsole/sample.h"
#include "../../../../../../libs/cpp/JunkCpp/src/Vector.h"

//
//#define DEF_FUNCS() \
//	int Func1(int a, int b) { \
//		int c; \
//		int d; \
//		return 0; \
//	}
//
//namespace Namespace1 {
//	extern "C" {
//		DEF_FUNCS()
//	}
//}

//extern "C++" {
//
//	__forceinline unsigned InterlockedIncrement(
//			unsigned volatile *Addend
//		)
//	{
//		return (unsigned)_InterlockedIncrement((volatile long*)Addend);
//	}
//}

struct Struct1 {
	int value;
	Struct1(int value) {
		this->value = value;
	}
	int operator+(const Struct1& c) const {
		return this->value + c.value;
	}
};

template<class T> struct TemplateStruct1 {
 	T value;
	TemplateStruct1(T value) {
		this->value = value;
	}
 	T operator+(const TemplateStruct1<T>& c) const {
 		return this->value + c.value;
 	}
};

class Foo {
public:
	void bar() const;
	void baz() volatile;
	void qux() const volatile;
};

int Func1();

namespace NS1 {
	int Func1();
	int Func2();
}

int Func1() {
	return 0;
}

int Func1();

int main() {
	auto a = Struct1(1);
	auto b = Struct1(2);
	auto c = a + b;

	auto ta = TemplateStruct1<int>(1);
	auto tb = TemplateStruct1<int>(2);
	auto tc = ta + tb;

	auto v1 = jk::Vector3d(1, 2, 3);
	auto v2 = jk::Vector3d(4, 5, 6);
	auto v3 = v1 + v2;

	Func1();
	NS1::Func1();
	NS1::Func2();
	// auto s1 = Struct1();
	// auto s2 = Struct1();
	// auto c = s1 + s2;

	// jk::Vector2f a, b, d;
	// d = a + b;


	//Namespace1::Struct2::InlineMethod1(1);
	return 0;
}


namespace NS1 {
	int Func1() {
		return 0;
	}
}

//namespace NS1 {
//	namespace {
//		struct AfeType {
//			template<class T> struct AfeTempl {
//				T Value;
//				template<class S> struct AfeTemplNested {
//					S Value;
//
//					void Method1() {
//						typedef AfeTempl<T>::AfeTemplNested<S> mytype;
//
//
//					}
//
//					template<class U> void TemplMethodInTempl(U value) {
//						int TemplMethodInTemplVar;
//					}
//				};
//			};
//		};
//	}
//}
//
//class Class1 {
//	int Variable1;
//
//};
//
//namespace NS2 {
//	typedef NS1::AfeType AfeTypeNew;
//
//}
//
//	static void __stdcall SendFilePathData(LPCWSTR szFilePathW, LPCSTR szFilePathA) {
//		NS1::AfeType::AfeTempl<int>::AfeTemplNested<int> afeVar;
//		afeVar.Method1();
//		afeVar.TemplMethodInTempl(32);
//
//}
