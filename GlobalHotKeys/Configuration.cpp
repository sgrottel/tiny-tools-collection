#include "pch.h"
#include "Configuration.h"
#include "StringUtils.h"
#include "SimpleLog/SimpleLog.hpp"

#include <yaml.h>

#include <unordered_map>
#include <memory>
#include <vector>
#include <algorithm>

namespace
{
	constexpr const wchar_t* c_regKeyApp = L"Software\\SGrottel\\GlobalHotkeys";
	constexpr const wchar_t* c_regValueConfigFilePath = L"configfile";

	class wruntime_error : public std::runtime_error {
	public:
		wruntime_error(const std::wstring& msg) : std::runtime_error("Error!"), message(msg) {};
		~wruntime_error() throw() {};

		std::wstring const& get_message() const { return message; }

	private:
		std::wstring message;
	};

	class YamlElement
	{
	public:
		using Ptr = std::shared_ptr<YamlElement>;

		YamlElement(size_t line, size_t col)
			: m_line{ line }, m_col{ col }
		{
			// intentionally empty
		}

		virtual ~YamlElement() = default;

		virtual void Add(Ptr e)
		{
			throw std::logic_error("Add not supported");
		}
		virtual void Finalize()
		{
			// intentionally empty
		}

		inline size_t GetLine() const
		{
			return m_line;
		}
		inline size_t GetColumn() const
		{
			return m_col;
		}

	private:
		size_t m_line;
		size_t m_col;
	};

	class YamlScalar: public YamlElement
	{
	public:
		using Ptr = std::shared_ptr<YamlScalar>;

		YamlScalar(size_t line, size_t col, std::wstring value)
			: YamlElement{ line, col }, m_value { value }
		{
		}
		virtual ~YamlScalar() = default;

		inline std::wstring const& Value() const
		{
			return m_value;
		}
	private:
		std::wstring m_value;
	};

	class YamlMapping : public YamlElement
	{
	public:
		using Ptr = std::shared_ptr<YamlMapping>;
		using Map_t = std::unordered_map<std::wstring, YamlElement::Ptr>;

		YamlMapping(size_t line, size_t col)
			: YamlElement{ line, col }
		{
			// intentionally empty
		}
		virtual ~YamlMapping() = default;

		inline Map_t& Value()
		{
			return m_map;
		}
		inline Map_t const& Value() const
		{
			return m_map;
		}

		void Add(YamlElement::Ptr e) override
		{
			if (m_nextKey.empty())
			{
				auto scalar = std::dynamic_pointer_cast<YamlScalar>(e);
				if (!scalar) throw std::logic_error("Expected scalar values for mapping key");
				m_nextKey = scalar->Value();
				return;
			}

			m_map[m_nextKey] = e;
			m_nextKey.clear();
		}

		void Finalize() override
		{
			if (!m_nextKey.empty())
			{
				throw new std::logic_error("Orphaned key");
			}
			for (auto p : m_map)
			{
				p.second->Finalize();
			}
		}

		YamlElement::Ptr Find(std::wstring const& key) const
		{
			auto i = m_map.find(key);
			if (i == m_map.end()) return nullptr;
			return i->second;
		}

	private:
		Map_t m_map;
		std::wstring m_nextKey;
	};

	class YamlSequence : public YamlElement
	{
	public:
		using Ptr = std::shared_ptr<YamlSequence>;
		using Sequence_t = std::vector<YamlElement::Ptr>;

		YamlSequence(size_t line, size_t col)
			: YamlElement{ line, col }
		{
			// intentionally empty
		}
		virtual ~YamlSequence() = default;

		inline Sequence_t& Value()
		{
			return m_sequence;
		}
		inline Sequence_t const& Value() const
		{
			return m_sequence;
		}

		void Add(YamlElement::Ptr e) override
		{
			m_sequence.push_back(e);
		}

		void Finalize() override
		{
			for (auto e : m_sequence)
			{
				e->Finalize();
			}
		}

	private:
		Sequence_t m_sequence;
	};

	class YamlParser
	{
	public:
		YamlParser(sgrottel::ISimpleLog& log)
			: m_init{ false }
		{
			if (yaml_parser_initialize(&m_parser) == 0)
			{
				log.Error("Failed to yaml_parser_initialize");
			}
			m_init = true;
		}

		~YamlParser()
		{
			yaml_parser_delete(&m_parser);
		}

		inline bool IsInitialized() const
		{
			return m_init;
		}

		void SetInputFile(FILE* file)
		{
			if (!m_init) throw new std::logic_error("Parser not initialized");
			yaml_parser_set_input_file(&m_parser, file);
		}

		YamlElement::Ptr Parse();

	private:
		class EventScope {
		public:
			EventScope(yaml_event_t& event)
				: m_event{ event }
			{
				// intentionally empty
			}

			~EventScope()
			{
				yaml_event_delete(&m_event);
			}

			operator yaml_event_t* ()
			{
				return &m_event;
			}

		private:
			yaml_event_t& m_event;
		};

		bool m_init;
		yaml_parser_t m_parser;
	};

	YamlElement::Ptr YamlParser::Parse()
	{
		bool inStream = false;
		bool inDocument = false;

		std::vector<YamlElement::Ptr> stack;
		auto topOfStack = [&stack]()
			{
				if (stack.empty()) return YamlElement::Ptr{ nullptr };
				return stack.back();
			};
		auto pushOnStack = [&stack](YamlElement::Ptr e)
			{
				stack.push_back(e);
			};
		YamlElement::Ptr lastElement;
		auto popFromStack = [&stack, &lastElement]()
			{
				if (stack.empty()) throw new std::logic_error("Parse stack depleted");
				lastElement = stack.back();
				stack.pop_back();
			};

		yaml_event_t event;
		yaml_event_type_t eventType = YAML_NO_EVENT;
		while (eventType != YAML_STREAM_END_EVENT)
		{
			if (!yaml_parser_parse(&m_parser, &event))
			{
				std::wstring error;
				switch (m_parser.error)
				{
				case YAML_NO_ERROR:
					error += L"No error is produced.";
					break;
				case YAML_MEMORY_ERROR:
					error += L"Cannot allocate or reallocate a block of memory.";
					break;
				case YAML_READER_ERROR:
					error += L"Cannot read or decode the input stream.";
					break;
				case YAML_SCANNER_ERROR:
					error += L"Cannot scan the input stream.";
					break;
				case YAML_PARSER_ERROR:
					error += L"Cannot parse the input stream.";
					break;
				case YAML_COMPOSER_ERROR:
					error += L"Cannot compose a YAML document.";
					break;
				case YAML_WRITER_ERROR:
					error += L"Cannot write to the output stream.";
					break;
				case YAML_EMITTER_ERROR:
					error += L"Cannot emit a YAML stream.";
					break;
				default:
					error += L"Unspecific error";
					break;
				}
				error += L"\n";
				error += ToW(m_parser.problem);
				error += L"\n\tat line "
					+ std::to_wstring(m_parser.problem_mark.line + 1)
					+ L", pos "
					+ std::to_wstring(m_parser.problem_mark.column + 1);
				throw wruntime_error{ error };
			}
			EventScope eventScope{ event };

			switch (eventType = event.type) {
			case YAML_STREAM_START_EVENT:
				inStream = true;
				break;
			case YAML_STREAM_END_EVENT:
				inStream = false;
				break;
			case YAML_DOCUMENT_START_EVENT:
				if (inStream) inDocument = true;
				break;
			case YAML_DOCUMENT_END_EVENT:
				inDocument = false;
				break;

			case YAML_SEQUENCE_START_EVENT:
			{
				if (!inDocument) continue;
				std::shared_ptr<YamlSequence> seq = std::make_shared<YamlSequence>(event.start_mark.line + 1, event.start_mark.column + 1);
				auto top = topOfStack();
				if (top) top->Add(seq);
				pushOnStack(seq);
				break;
			}
			case YAML_SEQUENCE_END_EVENT:
				if (!inDocument) continue;
				popFromStack();
				break;

			case YAML_MAPPING_START_EVENT:
			{
				if (!inDocument) continue;
				std::shared_ptr<YamlMapping> map = std::make_shared<YamlMapping>(event.start_mark.line + 1, event.start_mark.column + 1);
				auto top = topOfStack();
				if (top) top->Add(map);
				pushOnStack(map);
				break;
			}
			case YAML_MAPPING_END_EVENT:
				if (!inDocument) continue;
				popFromStack();
				break;

			case YAML_ALIAS_EVENT:
				throw wruntime_error{
					L"(line: "
					+ std::to_wstring(event.start_mark.line + 1)
					+ L", col: "
					+ std::to_wstring(event.start_mark.column + 1)
					+ L") Yaml anchors are not supported" };

			case YAML_SCALAR_EVENT:
			{
				if (!inDocument) continue;
				std::shared_ptr<YamlScalar> scalar = std::make_shared<YamlScalar>(event.start_mark.line + 1, event.start_mark.column + 1, ToW(event.data.scalar.value));
				auto top = topOfStack();
				if (top)
				{
					top->Add(scalar);
				}
				else
				{
					// empty stack means we are in neither a mapping nor a sequence
					// encountering a scalar in that situation means: this is it
					return scalar;
				}
				break;
			}

			}
		}

		lastElement->Finalize();
		return lastElement;
	}

	struct YamlElementReferenceException
	{
		YamlElement::Ptr element;
		std::exception innerException;
	};

	bool FindOptionalBool(YamlMapping::Ptr map, std::wstring const& key, bool defValue)
	{
		if (!map) return defValue;
		auto el = map->Find(key);
		if (!el) return defValue;
		YamlScalar::Ptr s = std::dynamic_pointer_cast<YamlScalar>(el);
		if (s)
		{
			std::wstring str = s->Value();
			if (str.empty()) return defValue;

			std::transform(str.begin(), str.end(), str.begin(), [](wchar_t w) { return std::tolower(w); });
			if (str == L"true") return true;
			if (str == L"false") return false;
			if (str == L"on") return true;
			if (str == L"off") return false;
			if (str == L"yes") return true;
			if (str == L"no") return false;
			if (str == L"0") return false;

			bool isNonNullNumber = true;
			bool allNull = true;
			for (wchar_t c : str)
			{
				if (c < L'0' || c > L'9')
				{
					isNonNullNumber = false;
					break;
				}
				else if (c != L'0')
				{
					allNull = false;
				}
			}
			if (allNull) isNonNullNumber = false;
			if (isNonNullNumber) return true;
		}

		std::string error{ "Entry `" + ToA(key) + "` must be a boolean value" };
		throw YamlElementReferenceException{
			el,
			std::invalid_argument(error)
		};
	}

}

Configuration::Configuration(sgrottel::ISimpleLog& log)
	: m_log{ log }, m_configFileLoaded{ false }
{
	LoadConfigFilePathFromRegistry();
	if (std::filesystem::is_regular_file(m_configFile))
	{
		m_configFileLoaded = SetFilePath(m_configFile);
	}
	else if (m_configFile.empty())
	{
		m_log.Write("No configuration file stored. Please `Select Configuration`");
	}
	else
	{
		m_log.Error(L"Stored configuration file not found: %s", m_configFile.wstring().c_str());
	}
}

Configuration::~Configuration()
{
}

bool Configuration::SetFilePath(std::filesystem::path const& path, std::optional<std::function<void(std::wstring const&)>> errorMessageReceiver)
{
	try
	{
		std::unique_ptr<FILE, decltype(&fclose)> file{ nullptr, &fclose };
		{
			FILE* rawFile;
			errno_t openErr = _wfopen_s(&rawFile, path.wstring().c_str(), L"r");
			if (openErr == 0)
			{
				file = std::move(std::unique_ptr<FILE, decltype(&fclose)>{ rawFile, & fclose });
			}
		}
		if (!file) {
			std::wstring error = L"file not found\n\t" + path.wstring();
			throw wruntime_error{ error };
		}

		YamlParser parser{ m_log };
		parser.SetInputFile(file.get());

		YamlElement::Ptr config = parser.Parse();
		if (!config) throw std::invalid_argument("Failed to parse configuration file: returned empty");

		YamlMapping::Ptr root = std::dynamic_pointer_cast<YamlMapping>(config);
		if (!root) throw std::invalid_argument("Configuration file root element is expected to be a mapping");

		bool bell = FindOptionalBool(root, L"bell", true);

		YamlElement::Ptr ghkeys = root->Find(L"globalhotkeys");
		if (!ghkeys) throw std::invalid_argument("Entry `globalhotkeys` not found");

		YamlSequence::Ptr ghkeysSeq = std::dynamic_pointer_cast<YamlSequence>(ghkeys);
		if (!ghkeysSeq) throw YamlElementReferenceException{ ghkeys, std::invalid_argument("`globalhotkeys` must be an array") };

		std::vector<HotKeyConfig> hotKeysConfig;

		for (auto ghkeysSeqEl : ghkeysSeq->Value())
		{
			YamlMapping::Ptr keyEl = std::dynamic_pointer_cast<YamlMapping>(ghkeysSeqEl);
			if (!keyEl) throw YamlElementReferenceException{ ghkeysSeqEl, std::invalid_argument("Entries in `globalhotkeys` must be mappings") };

			HotKeyConfig key;

			{
				YamlElement::Ptr vkcEl = keyEl->Find(L"code");
				if (!vkcEl) throw YamlElementReferenceException{ keyEl, std::invalid_argument("Entry in `globalhotkeys` must contain `code`") };
				YamlScalar::Ptr vkcScalar = std::dynamic_pointer_cast<YamlScalar>(vkcEl);
				if (!vkcScalar) throw YamlElementReferenceException{ vkcEl, std::invalid_argument("`globalhotkeys.code` must be a scalar value") };
				uint32_t code = HotKeyConfig::ParseVirtualKeyCode(vkcScalar->Value());
				if (code == HotKeyConfig::c_invalidVirtualKeyCode)
				{
					throw YamlElementReferenceException{ vkcScalar, std::invalid_argument(
						"Failed to parse `globalhotkeys.code: "
						+ ToA(vkcScalar->Value())
						+ "` as Virtual-Key code") };
				}
				key.virtualKeyCode = code;
			}

			key.modAlt = FindOptionalBool(keyEl, L"alt", false);
			key.modCtrl = FindOptionalBool(keyEl, L"ctrl", false);
			key.modShift = FindOptionalBool(keyEl, L"shift", false);

			{
				YamlElement::Ptr execEl = keyEl->Find(L"exec");
				if (!execEl) throw YamlElementReferenceException{ keyEl, std::invalid_argument("Entry in `globalhotkeys` must contain `exec`") };
				YamlScalar::Ptr execScalar = std::dynamic_pointer_cast<YamlScalar>(execEl);
				if (!execScalar) throw YamlElementReferenceException{ execEl, std::invalid_argument("`globalhotkeys.exec` must be a scalar value") };
				key.executable = execScalar->Value();
			}

			{
				YamlElement::Ptr workdirEl = keyEl->Find(L"workdir");
				if (workdirEl)
				{
					YamlScalar::Ptr workdirScalar = std::dynamic_pointer_cast<YamlScalar>(workdirEl);
					if (!workdirScalar) throw YamlElementReferenceException{ workdirEl, std::invalid_argument("`globalhotkeys.workdir` must be a scalar value") };
					key.workingDirectory = workdirScalar->Value();
				}
			}

			{
				YamlElement::Ptr argsEl = keyEl->Find(L"args");
				if (argsEl)
				{
					YamlSequence::Ptr argsSeq = std::dynamic_pointer_cast<YamlSequence>(argsEl);
					if (!argsSeq) throw YamlElementReferenceException{ argsSeq, std::invalid_argument("`globalhotkeys.args` must be a sequence") };
					for (YamlElement::Ptr argEl : argsSeq->Value())
					{
						YamlScalar::Ptr argScalar = std::dynamic_pointer_cast<YamlScalar>(argEl);
						if (!argScalar) throw YamlElementReferenceException{ argEl, std::invalid_argument("Entry in`globalhotkeys.args` must be a scalar values") };
						key.arguments.push_back(argScalar->Value());
					}
				}
			}

			key.noFileCheck = FindOptionalBool(keyEl, L"nofilecheck", false);

			hotKeysConfig.push_back(std::move(key));
		}

		// on success:
		{
			std::wstring report{ L"Successfully parsed configuration from:\n  " };
			report += path.wstring();
			report += L"\n  loaded ";
			report += std::to_wstring(hotKeysConfig.size());
			report += L" hotkey configurations.";
			for (auto const& hkc : hotKeysConfig)
			{
				report += L"\n    ";
				report += hkc.GetKeyWString();
				report += L" => ";
				report += hkc.executable;
			}

			m_log.Write(report);
		}
		m_hotKeys = hotKeysConfig;
		m_bell = bell;

		if (m_configFile != path)
		{
			m_configFile = path;
			SaveConfigFilePathInRegistry();
		}

		return true;
	}
	catch (YamlElementReferenceException& elEx)
	{
		std::string error{ "[line: "
				+ std::to_string(elEx.element->GetLine())
				+ ", col: "
				+ std::to_string(elEx.element->GetColumn())
				+ "] "
				+ elEx.innerException.what() };
		m_log.Error(("Failed parsing configuration: " + error).c_str());
		if (errorMessageReceiver.has_value())
		{
			errorMessageReceiver.value()(ToW(error.c_str()));
		}
	}
	catch (wruntime_error& wtrerr)
	{
		m_log.Error((L"Failed parsing configuration: " + wtrerr.get_message()).c_str());
		if (errorMessageReceiver.has_value())
		{
			errorMessageReceiver.value()(wtrerr.get_message());
		}
	}
	catch (std::exception& ex)
	{
		m_log.Error((L"Failed parsing configuration: " + ToW(ex.what())).c_str());
		if (errorMessageReceiver.has_value())
		{
			errorMessageReceiver.value()(ToW(ex.what()));
		}
	}
	catch (...)
	{
		m_log.Error(L"Failed parsing configuration: Unknown exception");
		if (errorMessageReceiver.has_value())
		{
			errorMessageReceiver.value()(L"Unknown exception");
		}
	}

	return false;
}

void Configuration::LoadConfigFilePathFromRegistry()
{
	DWORD size = 0;
	LSTATUS result = RegGetValueW(HKEY_CURRENT_USER, c_regKeyApp, c_regValueConfigFilePath, RRF_RT_REG_SZ, NULL, NULL, &size);
	if (result != ERROR_SUCCESS)
	{
		return;
	}

	std::vector<wchar_t> strBuf(size + 1, L'\0');
	size = static_cast<DWORD>(strBuf.size());
	result = RegGetValueW(HKEY_CURRENT_USER, c_regKeyApp, c_regValueConfigFilePath, RRF_RT_REG_SZ, NULL, strBuf.data(), &size);
	if (result != ERROR_SUCCESS)
	{
		return;
	}

	m_configFile = strBuf.data();
}

void Configuration::SaveConfigFilePathInRegistry()
{
	HKEY hKey;
	LSTATUS result = RegCreateKeyExW(HKEY_CURRENT_USER, c_regKeyApp, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
	if (result != ERROR_SUCCESS)
	{
		m_log.Error("Failed to save path in windows registry; RegCreateKeyExW: %u", static_cast<unsigned int>(result));
		return;
	}

	std::wstring str = m_configFile.wstring();
	result = RegSetValueExW(
		hKey,
		c_regValueConfigFilePath,
		0,
		REG_SZ,
		reinterpret_cast<LPBYTE>(str.data()),
		static_cast<DWORD>((str.length() + 1) * sizeof(wchar_t)));
	if (result != ERROR_SUCCESS)
	{
		m_log.Error("Failed to save path in windows registry; RegSetValueExW: %u", static_cast<unsigned int>(result));
		return;
	}

	RegCloseKey(hKey);
}
