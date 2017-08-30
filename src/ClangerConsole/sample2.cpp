//#include <Windows.h>
//#include "sample.h"

//int main() {
//	return 0;
//}

//#define DECLVAR() \
//	int g_A; \
//	int g_B; \
//	int g_C;
//
//DECLVAR();

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
