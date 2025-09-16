namespace AppSage.Web.Components.Filter
{
    public class FilterModel
    {
        /// A set of values that can be selected from
        public HashSet<string> Values { get;  } = new HashSet<string>();

        /// A set of values that have been selected
        public HashSet<string> SelectedValues { get;  } = new HashSet<string>();


        public void AddSelection(string selection)
        {
            if (selection == null) { throw new ArgumentNullException(nameof(selection)); }
            if(!Values.Contains(selection)) { throw new ArgumentException($"The selection '{selection}' is not in the list of available values."); }
            SelectedValues.Add(selection);
        }

        public void AddSelectionRange(IEnumerable<string> selections)
        {
            if (selections == null) { throw new ArgumentNullException(nameof(selections)); }
            foreach (var selection in selections)
            {
                if (!Values.Contains(selection)) { throw new ArgumentException($"The selection '{selection}' is not in the list of available values."); }
                SelectedValues.Add(selection);
            }
        }

        public void ClearSelectedSegments()
        {
            SelectedValues.Clear();
        }

    }
}
