//#pragma intrinsic(_InterlockedIncrement)
//extern "C" long _InterlockedIncrement(long volatile *);

#include <Windows.h>
//#include <vector>
//#include "../ClangerConsole/sample.h"
#include "../../../../../../libs/cpp/JunkCpp/src/Vector.h"

std::_Bool_struct<std::exception_ptr>::

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

int main() {
	auto a = Struct1();
	auto b = Struct1();
	auto c = a + b;
	// auto s1 = Struct1();
	// auto s2 = Struct1();
	// auto c = s1 + s2;

	// jk::Vector2f a, b, d;
	// d = a + b;


	//Namespace1::Struct2::InlineMethod1(1);
	return 0;
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
