using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu {

	public interface IJob: INotifyPropertyChanged {

		double Progress { get; }
		bool Running { get; }

		event EventHandler Done;

		void Start();
		void Abort();

	}

}
