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
// ---------------------------------
//
// version 0.1  -  2022-03-10
//
#ifndef _ConProgBar_h_included_AB27DE64_
#define _ConProgBar_h_included_AB27DE64_
#pragma once

#include <iostream>
#include <iomanip>
#include <chrono>
#include <stdexcept>
#include <sstream>

namespace sgconutil
{

	/**
	 * Console progress bar
	 *
	 * @tparam T the progress value.
	 *           This type must be cast-able to `float` and `double`
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
		typedef std::chrono::high_resolution_clock::time_point time_point;

		void COutReport();

		const unsigned int m_reportWidth;

		T m_minVal = static_cast<T>(0);
		T m_maxVal = static_cast<T>(1);
		T m_val = static_cast<T>(0);

		bool m_active = false;
		time_point m_startTime;
		time_point m_lastReportTime;
		float m_lastReportVal;
	};

	template<class T>
	ConProgBar<T>::ConProgBar(unsigned int reportWidth)
		: m_reportWidth(reportWidth) {
		// intentionally empty
	}

	template<class T>
	void ConProgBar<T>::Start(T minVal, T maxVal, T val) {
		if (m_active) throw std::logic_error("ConProgBar already running.");
		m_minVal = (std::min)(minVal, maxVal);
		m_maxVal = (std::max)(minVal, maxVal);
		m_val = (std::min)((std::max)(m_minVal, val), m_maxVal);
		m_active = true;
		m_startTime = std::chrono::high_resolution_clock::now();
		m_lastReportTime = std::chrono::high_resolution_clock::time_point{};
		m_lastReportVal = 2.0f;
		COutReport();
	}

	template<class T>
	void ConProgBar<T>::SetVal(T val) {
		if (!m_active) return;
		m_val = (std::min)((std::max)(m_minVal, val), m_maxVal);
		COutReport();
	}

	template<class T>
	void ConProgBar<T>::Abort() {
		if (!m_active) return;
		m_active = false;
		m_lastReportTime = std::chrono::high_resolution_clock::time_point{};
		COutReport();
	}

	template<class T>
	void ConProgBar<T>::Complete() {
		if (!m_active) return;
		m_val = m_maxVal;
		m_active = false;
		m_lastReportTime = std::chrono::high_resolution_clock::time_point{};
		COutReport();
	}

	template<class T>
	void ConProgBar<T>::COutReport() {
		float v = (static_cast<float>(m_val) - static_cast<float>(m_minVal))
			/ (static_cast<float>(m_maxVal) - static_cast<float>(m_minVal));
		auto now = std::chrono::high_resolution_clock::now();
		float dur = std::chrono::duration<float>(now - m_startTime).count();
		auto lastRepDur = now - m_lastReportTime;

		constexpr const float reportPrecision = 1000.0f;
		constexpr const auto minReportUpdateTimeSpanSec = 100ms;

		if (lastRepDur < minReportUpdateTimeSpanSec) {
			// don't spam the console.
			return;
		}

		// relevant update: report
		m_lastReportTime = now;
		float eta = 0.0f;
		if (dur > 1.0f && v > 0.0001f) {
			// assume very simple linear progress to estimate ETA
			eta = (dur / v) - dur;
		}

		auto timeToString = [](float timeVal) -> std::string {
			std::stringstream timeRep;
			if (timeVal <= 60) {
				timeRep << std::fixed << std::setprecision(1) << timeVal << "s";
			}
			else {
				unsigned int sec = static_cast<unsigned int>(timeVal) % 60;
				unsigned int intTime = static_cast<unsigned int>(timeVal) / 60;
				if (intTime <= 60) {
					timeRep << intTime << ":" << std::setw(2) << std::setfill('0') << sec;
				}
				else {
					unsigned int m = intTime % 60;
					intTime /= 60;
					if (intTime < 10) {
						// less than 10 hours
						timeRep << intTime << ":" << std::setw(2) << std::setfill('0') << m << "h";
					}
					else {
						// 10h or more. No need for any more precision.
						timeRep << intTime << "h";
					}
				}
			}
			return timeRep.str();
		};
		auto largeIntToString = [](unsigned long long v) -> std::string {
			if (v > 1000000u) {
				return (std::stringstream{} << std::fixed << std::setprecision(1) << (static_cast<float>(v) / 1000000.0f) << "M").str();
			}
			else if (v > 1000) {
				return (std::stringstream{} << std::fixed << std::setprecision(1) << (static_cast<float>(v) / 1000.0f) << "K").str();
			}
			else {
				return std::to_string(v);
			}
		};
		if (m_active) {
			std::string valStr = largeIntToString(m_val) + "/" + largeIntToString(m_maxVal);
			std::string perStr = (std::stringstream{} << std::fixed << std::setprecision(1) << (100.0f * v) << "%").str();
			bool showVal = true;
			bool showPer = true;

			if (showVal && showPer && (valStr.length() + perStr.length() + 1) * 4 > m_reportWidth) {
				// only show duration or eta alternating
				if ((static_cast<int>(dur) % 4) % 2 == 0) {
					showVal = false;
				}
				else {
					showPer = false;
				}
				size_t l = (std::max)(valStr.length(), perStr.length());
				valStr.resize(l, ' '); // pad right, so there will be no jumping
				perStr.resize(l, ' ');
			}

			std::string durStr = "dur: " + timeToString(dur);
			std::string etaStr = "eta:-" + timeToString(eta);
			bool showDur = true;
			bool showEta = (eta > 0);

			if (showEta && showDur && (durStr.length() + etaStr.length() + 1) * 3 > m_reportWidth) {
				// only show duration or eta alternating
				if (static_cast<int>(dur) % 4 > 1) {
					showEta = false;
				}
				else {
					showDur = false;
				}
				size_t l = (std::max)(durStr.length(), etaStr.length());
				durStr.resize(l, ' '); // pad right, so there will be no jumping
				etaStr.resize(l, ' ');
			}

			unsigned int barLen = m_reportWidth;
			if (showVal) {
				barLen -= static_cast<unsigned int>(valStr.length());
			}
			if (showPer) {
				barLen -= static_cast<unsigned int>(perStr.length());
				if (showVal) {
					barLen--; // one more space inbetween
				}
			}
			if (showDur) {
				barLen -= static_cast<unsigned int>(durStr.length());
			}
			if (showEta) {
				barLen -= static_cast<unsigned int>(etaStr.length());
				if (showDur) {
					barLen--; // one more space inbetween
				}
			}
			barLen -= 4;
			const unsigned int fillBarLen = static_cast<unsigned int>(static_cast<float>(barLen) * v);

			std::cout << "\r";
			if (showVal) {
				std::cout << valStr;
				if (showPer) {
					std::cout << " ";
				}
			}
			if (showPer) {
				std::cout << perStr;
			}
			std::cout << " [" << std::setw(fillBarLen) << std::setfill('=') << ""
				<< std::setw(barLen - fillBarLen) << std::setfill(' ') << "] ";
			if (showDur) {
				std::cout << durStr;
				if (showEta) {
					std::cout << " ";
				}
			}
			if (showEta) {
				std::cout << etaStr;
			}

		}
		else {
			// clear
			std::cout << "\r" << std::setw(m_reportWidth) << std::setfill(' ') << "\r";
			// report
			if (m_val == m_maxVal) {
				std::cout << "Completed " << m_maxVal << " in " << timeToString(dur) << "\n";
			}
			else {
				std::cout  << "Aborted at " << m_val << "/" << m_maxVal << " (" << std::fixed << std::setprecision(1) << (100.0f * v) << "%) after " << timeToString(dur) << "\n";
			}
		}

	}

}

#endif  _ConProgBar_h_included_AB27DE64_
