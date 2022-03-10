//
// ConProgBar.h
// A simple single-file C++/H progress bar for the text console.
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
#ifndef _ConProgBar_h_included_AB27DE64_
#define _ConProgBar_h_included_AB27DE64_
#pragma once

#include <iostream>
#include <iomanip>
#include <chrono>
#include <stdexcept>

namespace sgconutil
{

	/**
	 * Console progress bar
	 *
	 * @tparam T the progress value
	 */
	template<class T>
	class ConProgBar
	{
	public:
		ConProgBar(unsigned int reportWidth = 70u);

		/**
		 * Starts progress reporting.
		 * You should take care that nothing writes to std::cout during normal operations
		 */
		void Start(T minVal, T maxVal, T val);

		void SetVal(T val);

		/**
		 * Stops progress reporting
		 */
		void Abort();

		/**
		 * Sets value to maxValue and stops progress reporting
		 */
		void Complete();

		inline T const& GetMinValue() const {
			return m_minVal;
		}
		inline T const& GetMaxValue() const {
			return m_maxVal;
		}
		inline T const& GetValue() const {
			return m_val;
		}

	private:

		const unsigned int m_reportWidth;

		T m_minVal = static_cast<T>(0);
		T m_maxVal = static_cast<T>(1);
		T m_val = static_cast<T>(0);

		bool m_active = false;
	};

	template<class T>
	ConProgBar<T>::ConProgBar(unsigned int reportWidth = 70u)
		: m_reportWidth(reportWidth) {
		// intentionally empty
	}

	template<class T>
	void ConProgBar<T>::Start(T minVal, T maxVal, T val) {
		if (m_active) throw std::logic_error("ConProgBar already running.");
		m_minVal = minVal;
		m_maxVal = maxVal;
		m_val = (std::min)((std::max)(minVal, val), maxVal);
		m_active = true;
	}

	template<class T>
	void ConProgBar<T>::SetVal(T val) {
		if (!m_active) return;
		m_val = (std::min)((std::max)(m_minVal, val), m_maxVal);
		std::cout << "\r" << m_val;
	}

	template<class T>
	void ConProgBar<T>::Abort() {
		if (!m_active) return;
		std::cout << " Abort" << std::endl;
		m_active = false;
	}

	template<class T>
	void ConProgBar<T>::Complete() {
		if (!m_active) return;
		std::cout << "\r" << m_maxVal << " Complete" << std::endl;
		m_active = false;
	}

}

#endif  _ConProgBar_h_included_AB27DE64_
