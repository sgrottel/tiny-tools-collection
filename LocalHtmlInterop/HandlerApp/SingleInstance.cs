
namespace LocalHtmlInterop.Handler
{

	/// <summary>
	/// Utility class to ensure only a single instance of this application is running
	/// </summary>
	public class SingleInstance
	{

		private Mutex? m;

		public SingleInstance()
		{
			string id = AppDomain.CurrentDomain.FriendlyName;
			m = new Mutex(false, id);
			if (!m.WaitOne(0))
			{
				m.Dispose();
				m = null;
			}
		}

		/// <summary>
		/// Returns `true` if failed to capture the "single application" mutex
		/// </summary>
		public bool Failed {
			get {
				return m == null;
			}
		}

	}
}
