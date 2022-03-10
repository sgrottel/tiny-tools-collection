//
// ConProgBar
// ConProgBar.cpp : simple test application
//
// Copyright 2022 SGrottel
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissionsand
// limitations under the License.
//
#include "ConProgBar.h"

#include <thread>
#include <chrono>

using namespace std::chrono_literals;

int main()
{
	std::cout << "ConProgBar Test Application\n";

	sgconutil::ConProgBar<int> bar;

	std::cout << "\nProgress in 10 steps of 100ms:\n";
	bar.Start(0, 10, 0);
	for (int i = 0; i < 10; ++i) {
		bar.SetVal(i);
		std::this_thread::sleep_for(100ms);
	}
	bar.Complete();

	std::cout << "\nProgress in 100 steps of 100ms:\n";
	bar.Start(0, 100, 0);
	for (int i = 0; i < 100; ++i) {
		bar.SetVal(i);
		std::this_thread::sleep_for(100ms);
	}
	bar.Complete();

	std::cout << "\nProgress in 10000 steps of 100ms and abort at 50:\n";
	bar.Start(0, 10000, 0);
	for (int i = 0; i < 10000; ++i) {
		bar.SetVal(i);
		if (i == 50) {
			bar.Abort();
			break;
		}
		std::this_thread::sleep_for(100ms);
	}
	bar.Complete(); // will be noop, because of previous abort

	std::cout << "\nProgress in 200 steps of ~1ms:\n";
	bar.Start(0, 200, 0);
	for (int i = 0; i < 200; ++i) {
		bar.SetVal(i);
		std::this_thread::sleep_for(1ms);
	}
	bar.Complete();

	std::cout << "\nDone.\n" << std::endl;
	return 0;
}
