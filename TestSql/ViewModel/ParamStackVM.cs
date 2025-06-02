using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSql.Model;
using TestSql.ViewModel.Commands;
using TestSql.ViewModel.Helpers;

namespace TestSql.ViewModel
{
    public class ParamStackVM : INotifyPropertyChanged
    {
        public ObservableCollection<CategoryItem> CategoryItems { get; set; }
        public ObservableCollection<ParamStack> ParamStacks { get; set; }

		private CategoryItem selectedCategoryItem;

        public CategoryItem SelectedCategoryItem
        {
			get { return selectedCategoryItem; }
			set 
			{ 
				selectedCategoryItem = value; 
				OnPropertyChanged("SelectedCategoryItem");
				//TODO: get paramStacks for selected CategoryItem 
				GetParamStacks();
			}
		}

		public NewCategoryItemCommand NewCategoryItemCommand { get; set; }
		public NewParamStackCommand NewParamStackCommand { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public ParamStackVM()
        {
			NewCategoryItemCommand = new NewCategoryItemCommand(this);
			NewParamStackCommand = new NewParamStackCommand(this);

			CategoryItems = new ObservableCollection<CategoryItem>();
			ParamStacks = new ObservableCollection<ParamStack>();

			GetCategoryItems();
        }

		public void CreateCategoryItem()
		{
			CategoryItem newCategoryItem = new CategoryItem()
			{
				Name = "NewCategory"
			};

            DatabaseHelper.Insert(newCategoryItem);

			GetCategoryItems();
        }

		public void CreateParamStack(int categoryItemId)
		{
			ParamStack newStack = new ParamStack()
			{
				CategoryItemId = categoryItemId
				//TODO: Add TypeFirst + TypeSecond
			};

			DatabaseHelper.Insert(newStack);

			GetParamStacks();
		}

		private void GetCategoryItems()
		{
			var categotyItems = DatabaseHelper.Read<CategoryItem>();

			CategoryItems.Clear();
			foreach(var categoryItem in categotyItems)
			{
				CategoryItems.Add(categoryItem);
			}
		}
        private void GetParamStacks()
        {
			if (SelectedCategoryItem != null)
			{
				var paramStacks = DatabaseHelper.Read<ParamStack>().Where(n => n.CategoryItemId == SelectedCategoryItem.Id).ToList();

				ParamStacks.Clear();
				foreach (var paramStack in paramStacks)
				{
					ParamStacks.Add(paramStack);
				}
			}
        }
		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
    }
}
