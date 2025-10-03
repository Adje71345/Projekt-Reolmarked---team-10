```mermaid
classDiagram
direction LR

%% =====================
%% MODELS
%% =====================
namespace Models {
    class MonthlyStatement {
        + StatementId : int
        + RenterId : int
        + Month : int
        + Year : int
        + TotalSales : double
        + Commission : double
        + RentalFee : double
        + NetResult : double
        + CreatedDate : DateTime
        + enum Status
    }

    class PaymentMethod {
        + PaymentMethodId : int
        + Name : string
    }

    class Rack {
        + RackId : int
        + RackStatusId : int
        + StatusName : string
    }

    class RentalContract {
        + RentalId : int
        + RenterId : int
        + RackId : int
        + StartDate : DateOnly
        + EndDate : DateOnly?
    }

    class Renter {
        + RenterId : int
        + FirstName : string
        + LastName : string
        + Email : string
        + Phone : string
        + PaymentMethodId : int
    }

    class SaleLine {
        + SaleLineId : int
        + SaleDate : DateTime
        + Price : decimal
        + RackId : int
        + ScanCode : string
        + BarcodeImage : ImageSource?
    }
}

%% =====================
%% INTERFACES
%% =====================
namespace Interfaces {
    class IRepository~T~ {
        <<interface>>
        + GetAll() IEnumerable~T~
        + GetById(id:int) T
        + Add(entity:T) void
        + Update(entity:T) void
        + Delete(id:int) void
    }

    class IRackRepository {
        <<interface>>
        + GetAvailableRacks() IEnumerable~Rack~
        + GetOccupiedRacks() IEnumerable~Rack~
        + GetCount() int
        + UpdateRackStatus(rackId:int,newStatus:int) void
        + UpdateStatusesForEndedContracts() void
    }

    class IRentalContractRepository {
        <<interface>>
        + GetActiveContractByRack(rackId:int) RentalContract
        + GetActiveContractsByRenter(renterId:int) IEnumerable~RentalContract~
        + GetActiveRentalContractsByMonth(year:int,month:int) IEnumerable~RentalContract~
    }

    class IRenterRepository {
        <<interface>>
        + GetCount() int
    }

    class ISaleLineRepository {
        <<interface>>
        + GetSalesForRackWithActiveContractLastMonth(rackId:int) IEnumerable~SaleLine~
        + GetTotalSalesToday() decimal
        + AddMany(lines:IEnumerable~SaleLine~) void
    }
}

%% =====================
%% REPOSITORIES
%% =====================
namespace Repositories {
    class PaymentMethodRepository {
        - _connectionString : string
        + GetAll() IEnumerable~PaymentMethod~
        + GetById(id:int) PaymentMethod
        + Add(entity:PaymentMethod) void
        + Update(entity:PaymentMethod) void
        + Delete(id:int) void
    }

    class RackRepository {
        - _connectionString : string
        + GetAll() IEnumerable~Rack~
        + GetById(id:int) Rack
        + Add(rack:Rack) void
        + Update(rack:Rack) void
        + Delete(id:int) void
        + GetAvailableRacks() IEnumerable~Rack~
        + GetOccupiedRacks() IEnumerable~Rack~
        + GetCount() int
        + UpdateRackStatus(rackId:int,newStatus:int) void
        + UpdateStatusesForEndedContracts() void
    }

    class RentalContractRepository {
        - _connectionString : string
        + GetAll() IEnumerable~RentalContract~
        + GetById(id:int) RentalContract
        + Add(contract:RentalContract) void
        + Update(contract:RentalContract) void
        + Delete(id:int) void
        + GetActiveContractByRack(rackId:int) RentalContract
        + GetActiveContractsByRenter(renterId:int) IEnumerable~RentalContract~
        + GetActiveRentalContractsByMonth(year:int,month:int) IEnumerable~RentalContract~
    }

    class RenterRepository {
        - _connectionString : string
        + GetAll() IEnumerable~Renter~
        + GetById(id:int) Renter
        + Add(renter:Renter) void
        + Update(renter:Renter) void
        + Delete(id:int) void
        + GetCount() int
    }

    class SaleLineRepository {
        - _connectionString : string
        - _rentalContractRepository : RentalContractRepository
        + GetAll() IEnumerable~SaleLine~
        + GetById(id:int) SaleLine
        + Add(line:SaleLine) void
        + Update(line:SaleLine) void
        + Delete(id:int) void
        + AddMany(lines:IEnumerable~SaleLine~) void
        + GetTotalSalesToday() decimal
        + GetSalesForRackWithActiveContractLastMonth(rackId:int) IEnumerable~SaleLine~
    }
}

%% =====================
%% VIEWMODELS
%% =====================
namespace ViewModels {
    class ViewModelBase {
        <<abstract>>
        + PropertyChanged : event
        + OnPropertyChanged(name:string) void
        + SetProperty<T>(ref field:T, value:T) bool
    }

    class AddRenterViewModel {
        + FirstName : string
        + LastName : string
        + PhoneNumber : string
        + Email : string
        + PaymentMethodId : int
        + AddRenterCommand : ICommand
        + CancelCommand : ICommand
    }

    class AddRentContractViewModel {
        + RackId : int
        + SelectedRenter : Renter
        + RenterId : int
        + StartDateTime : DateTime?
        + EndDateTime : DateTime?
        + NoEnd : bool
        + SubmitCommand : ICommand
        + CloseCommand : ICommand
    }

    class DashBoardViewModel {
        + SalesToday : decimal
        + OccupancyPercentage : int
        + RenterCount : int
        + NavigateCommand : ICommand
    }

    class EndRentContractViewModel {
        + RackId : int
        + RenterId : int
        + RenterDisplay : string
        + TerminationDate : DateTime
        + SubmitCommand : ICommand
        + CloseCommand : ICommand
    }

    class RackViewModel {
        + Racks : ObservableCollection~Rack~
        + RackSlots : ObservableCollection~RackSlot~
        + SelectedRack : Rack
        + CurrentRackPanel : ViewModelBase
        + SelectRackCommand : ICommand
        + SelectRackByIdCommand : ICommand
    }

    class RenterViewModel {
        + Renters : ObservableCollection~Renter~
        + SearchText : string
        + SelectedRenter : Renter
        + IsInfoPanelOpen : bool
        + IsAddPanelOpen : bool
    }

    class MonthlyStatementViewModel {
        + Years : ObservableCollection~int~
        + Months : ObservableCollection~string~
        + Rows : ObservableCollection~MonthlyStatementRow~
        + SelectedYear : int
        + SelectedMonth : string
        + PeriodText : string
    }
}

%% =====================
%% RELATIONER MELLEM LAG
%% =====================

%% Models
Renter --> PaymentMethod : has
RentalContract --> Renter : FK
RentalContract --> Rack : FK
SaleLine --> Rack : FK
MonthlyStatement --> Renter : FK

%% Interfaces (arv fra generic interface)
IRepository~T~ <|-- IRackRepository : extends
IRepository~T~ <|-- IRentalContractRepository : extends
IRepository~T~ <|-- IRenterRepository : extends
IRepository~T~ <|-- ISaleLineRepository : extends

%% Repositories implements interfaces
PaymentMethodRepository <|.. IRepository~PaymentMethod~ : implements
RackRepository <|.. IRackRepository : implements
RentalContractRepository <|.. IRentalContractRepository : implements
RenterRepository <|.. IRenterRepository : implements
SaleLineRepository <|.. ISaleLineRepository : implements

%% Repo -> Model (association)
PaymentMethodRepository --> PaymentMethod : manages
RackRepository --> Rack : manages
RentalContractRepository --> RentalContract : manages
RenterRepository --> Renter : manages
SaleLineRepository --> SaleLine : manages

%% ViewModels -> Repos/Models (dependency)
AddRenterViewModel ..> IRenterRepository : uses
AddRenterViewModel ..> IRepository~PaymentMethod~ : uses
AddRentContractViewModel ..> Renter : uses
AddRentContractViewModel ..> PaymentMethod : uses
AddRentContractViewModel ..> IRentalContractRepository : uses
DashBoardViewModel ..> IRenterRepository : uses
DashBoardViewModel ..> IRackRepository : uses
DashBoardViewModel ..> IRentalContractRepository : uses
DashBoardViewModel ..> ISaleLineRepository : uses
EndRentContractViewModel ..> IRentalContractRepository : uses
EndRentContractViewModel ..> IRenterRepository : uses
RackViewModel ..> IRackRepository : uses
RackViewModel ..> IRentalContractRepository : uses
RackViewModel ..> IRenterRepository : uses
RackViewModel ..> IRepository~PaymentMethod~ : uses
RenterViewModel ..> IRenterRepository : uses
RenterViewModel ..> IRentalContractRepository : uses
RenterViewModel ..> ISaleLineRepository : uses
RenterViewModel ..> IRepository~PaymentMethod~ : uses
MonthlyStatementViewModel ..> IRenterRepository : uses
MonthlyStatementViewModel ..> IRentalContractRepository : uses
MonthlyStatementViewModel ..> ISaleLineRepository : uses

