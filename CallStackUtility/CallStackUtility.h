//
// CallStackUtility.h
// Open Source via MIT License
//

#pragma once

#include <string>
#include <sstream>
#include <cstdint>
#include <vector>
#include <stdexcept>
#include <mutex>

namespace callstackutility
{

	/// Represents a callstack
	class CallStack
	{
	public:

		/// Class representing a single frame of the call stack
		class Frame
		{
		public:

			/// \return The code stack address in program memory space
			inline uint64_t getAddress() const { return m_address; }

			/// \return Function name, including namespace and class, if applicable
			inline std::string const& getFunction() const { return m_function; }

			/// \return The source code file if available
			inline std::string const& getFile() const { return m_file; }

			/// \return The source code line if available
			inline uint64_t getLine() const { return m_line; }

			/// \return The code symbol address
			inline uint64_t getSymbolAddress() const { return m_symAddress; }

			/// \return The name of the PE image containing the function
			inline std::string const& getImageName() const { return m_imageName; }

			/// \return The base address of the PE image
			inline uint64_t getImageBaseAddress() const { return m_imageBaseAddress; }

		private:

			/// The code stack address in program memory space
			uint64_t m_address;

			/// Function name, including namespace and class, if applicable
			std::string m_function;

			/// The source code file if available
			std::string m_file;

			/// The source code line if available
			uint64_t m_line;

			/// The code symbol address
			uint64_t m_symAddress;

			/// The name of the PE image containing the function
			std::string m_imageName;

			/// The base address of the PE image
			uint64_t m_imageBaseAddress;

			friend class CallStack;
		};

		/// Typedef alias
		typedef std::vector<Frame>::const_iterator const_iterator;

		/// Creates a callstack of the current invocation
		/// \remarks
		/// On Nvidia GeForce drivers with activated "Thread Optimization" (3D settings) the
		/// callstack might end in a thread inside the graphics driver.
		static CallStack Capture();

		/// Ctor & Dtor
		CallStack() = default;
		~CallStack() = default;

		/// Number of frames on the callstack
		inline size_t size() const { return m_frames.size(); }

		/// Access the idx'th frame of the callstack, zero being the top-most frame
		inline Frame const& operator[](size_t idx) const
		{
			if (idx < 0 || idx >= m_frames.size())
				throw std::logic_error("CallStack frame access index out of bounds");
			return m_frames[idx];
		}

		/// `begin` and `end` for range-based loop
		inline const_iterator begin() const { return m_frames.begin(); }
		inline const_iterator end() const { return m_frames.end(); }

		/// Internal class to manage windows API access
		class Api;

	private:

		/// Internal implementation of loading a call stack, not cleaned up yet
		static CallStack Load(Api const& api);

		/// Initializing Ctor
		/// Copies the frames to the internal member. Using this ctor instead of direct access
		/// to `m_frames` allows us to easily change the internal state if we ever have to.
		CallStack(std::vector<Frame> frames);

		/// Frames of the callstack
		std::vector<Frame> m_frames;
	};

	/// Utility function to stream a callstack frame
	template <class Stream>
	Stream& operator<<(Stream& stream, ::callstackutility::CallStack::Frame const& frame)
	{
		if (!frame.getFunction().empty())
			stream << frame.getFunction() << " ";
		if (!frame.getImageName().empty())
			stream << "(" << frame.getImageName() << ":" << frame.getImageBaseAddress() << ") ";
		else if (frame.getImageBaseAddress() != 0)
			stream << "(" << frame.getImageBaseAddress() << ") ";
		if (!frame.getFile().empty())
			stream << "[" << frame.getFile() << ":" << frame.getLine() << "] ";
		if (frame.getSymbolAddress() != 0)
			stream << frame.getSymbolAddress() << " ";
		stream << frame.getAddress();

		return stream;
	}

	/// Utility function to format a callstack frame as string
	inline std::string to_string(::callstackutility::CallStack::Frame const& frame)
	{
		std::stringstream s;
		s << frame;
		return s.str();
	}

}
