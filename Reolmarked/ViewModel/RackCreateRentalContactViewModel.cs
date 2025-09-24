using Reolmarked.Model;

namespace Reolmarked.ViewModel
{
    public class RackCreateRentalContactViewModel : ViewModelBase
    {
        public Rack Rack { get; }
        public RackCreateRentalContactViewModel(Rack rack) => Rack = rack;
    }
}
