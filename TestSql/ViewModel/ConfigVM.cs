using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSql.Model;
using TestSql.ViewModel.Commands;

namespace TestSql.ViewModel
{
    public class ConfigVM
    {
		private Config configuration;

		public Config Configuration
        {
			get { return configuration; }
			set { configuration = value; }
		}

		public CreateConfigCommand ConfigCommand { get; set; }
        public DeleteConfigCommand DeleteCommand { get; set; }

        public ConfigVM()
        {
            ConfigCommand = new CreateConfigCommand(this);
            DeleteCommand = new DeleteConfigCommand(this);
        }
    }
}
