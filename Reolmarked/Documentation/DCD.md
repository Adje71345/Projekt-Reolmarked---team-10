```mermaid
classDiagram
    %% UC1
    AddRenterViewModel ..> IRenterRepository : uses
    AddRenterViewModel ..> Renter : creates
    AddRenterViewModel ..> PaymentMethod : uses
    IRenterRepository ..> Renter : manages
    Renter --> PaymentMethod : has

    %% UC2
    RenterViewModel ..> IRenterRepository : uses
    RenterViewModel ..> Renter : uses
    IRenterRepository ..> Renter : manages

    %% UC3
    RackViewModel ..> IRackRepository : uses
    RackViewModel ..> Rack : uses
    IRackRepository ..> Rack : manages

    %% UC4
    AddRentContractViewModel ..> IRentalContractRepository : uses
    AddRentContractViewModel ..> Renter : uses
    AddRentContractViewModel ..> Rack : uses
    AddRentContractViewModel ..> PaymentMethod : uses
    IRentalContractRepository ..> RentalContract : manages
    RentalContract --> Rack : has
    RentalContract --> Renter : has
    Renter --> PaymentMethod : has

    %% UC5
    EndRentContractViewModel ..> IRentalContractRepository : uses
    EndRentContractViewModel ..> IRenterRepository : uses
    EndRentContractViewModel ..> RentalContract : uses
    IRentalContractRepository ..> RentalContract : manages
    RentalContract --> Rack : has
    RentalContract --> Renter : has

    %% UC6
    LabelViewModel ..> Rack : uses

    %% UC7
    SaleViewModel ..> ISaleLineRepository : uses
    SaleViewModel ..> SaleLine : uses
    ISaleLineRepository ..> SaleLine : manages
    SaleLine --> Rack : has

    %% UC8
    MonthlyStatementViewModel ..> MonthlyStatementRow : uses
    MonthlyStatementRow ..> Renter : displays
