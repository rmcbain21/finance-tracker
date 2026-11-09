# Finance Tracker

A personal app for one person to record what they spend, from grocery receipts to gas fill-ups to recurring charges, and to see how prices and spending change over time.

**Owner**:
The one person who uses the app and whose spending it records.
_Avoid_: User, customer, account

## Purchases

**Chain**:
The business a Store belongs to, such as Walmart. A Chain may have only one Store, like a local farmers' market.
_Avoid_: Brand, company, retailer

**Store**:
One location where purchases are made, such as Walmart on Main St or a gas station, belonging to a Chain. Its country sets the currency and default fuel unit of everything bought there.
_Avoid_: Merchant, vendor, shop, location (as a separate term)

**Receipt**:
One purchase at a Store on a date, with the Receipt Lines bought, any receipt-wide tax and discount, and the total actually paid. The total is what counts toward spending. A Receipt with no Receipt Lines (such as a haircut) carries its own Category.
_Avoid_: Transaction, purchase, trip, expense

**Receipt Line**:
One line of a Receipt: a Store Item, the quantity bought, the price actually paid after any line discount, and a Category.
_Avoid_: Line item, item, row

**Refund**:
A Receipt with a negative total for things returned to a Store. It reduces spending in the month it happens and never counts toward price history.
_Avoid_: Return, credit, negative receipt

**Draft Receipt**:
A Receipt saved quickly at checkout (Store, total, Receipt Photo) whose Receipt Lines are not filled in yet. Its total counts toward spending as Uncategorized until it is finished.
_Avoid_: Incomplete receipt, quick entry

**Receipt Photo**:
An image of the paper receipt, attached to a Receipt as its record.
_Avoid_: Scan, attachment, image

## Products and prices

**Store Item**:
Something one Chain sells under one SKU, with its receipt name, pack size and unit, shared by all of that Chain's Stores. It can be a stub holding only the SKU until its details are filled in.
_Avoid_: SKU (for the thing itself), item, product listing

**Product**:
The store-independent thing whose price is compared across Stores, such as "2% Milk". Store Items may be linked to a Product; linking is optional.
_Avoid_: Ingredient, item, generic

**Comparison Unit**:
The one unit a Product's prices are compared in, such as $/egg or $/lb. Every Store Item linked to the Product must use a unit convertible to it.
_Avoid_: Base unit, standard unit

**Unit Price**:
The price per unit of measure (such as $/gal or $/kg) of a Receipt Line or Fill-up, used to compare prices across pack sizes and Stores.
_Avoid_: Price per, normalized price

## Gas

**Fill-up**:
One fuel purchase at a Store: fuel grade, volume, price per unit, total, whether the tank was filled to full, and optionally the Vehicle and its odometer reading. It holds only fuel; anything else bought at the same time is a separate Receipt.
_Avoid_: Gas receipt, refuel, tank

**Fuel Economy**:
Distance per volume of fuel (MPG or L/100 km) for a Vehicle, measured between two full Fill-ups and including any partial Fill-ups between them.
_Avoid_: Mileage, efficiency

**Vehicle**:
A car or other vehicle that Fill-ups are made for, so fuel economy and cost per distance can be tracked.
_Avoid_: Car, auto

## Recurring

**Recurring Charge**:
A charge that repeats on a schedule, such as a streaming service, insurance or a utility bill, with a payee, Category, amount and currency. It is either Fixed (same amount each period) or Variable (the amount changes each period). It can be ended on a date and keeps its history.
_Avoid_: Subscription, bill, repeating expense (Subscription and Bill are Categories)

**Pending Charge**:
One period's expected charge from a Recurring Charge, waiting for the Owner to confirm it (editing the amount if needed) or skip it. Once confirmed it is a permanent record of what was paid.
_Avoid_: Upcoming charge, scheduled payment, instance

## Spending

**Category**:
A label from an editable list, such as Groceries or Household, that spending is grouped under. Receipt Lines carry Categories, except on a Receipt with no lines; a Fill-up is always Gas.
_Avoid_: Tag, type, bucket

**Home Currency**:
The currency the app reports in by default (USD). Amounts in other currencies are kept as they were paid and are never converted.
_Avoid_: Base currency, default currency
