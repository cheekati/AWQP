# API Endpoint Catalog

All protected endpoints require `Authorization: Bearer <token>`.

## Authentication

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/auth/login` | Authenticate and issue JWT/refresh token |
| POST | `/api/auth/refresh` | Rotate refresh token and issue new JWT |

## Customer and product

| Method | Endpoint | Roles |
| --- | --- | --- |
| GET | `/api/customers` | Admin, Sales Team, Production Manager |
| POST | `/api/customers` | Admin, Sales Team |
| GET | `/api/products` | Admin, Sales Team, Production Manager, Production Engineer, Quality Engineer |
| POST | `/api/products` | Admin, Production Engineer |

## Sales workflow

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/sales/rfqs` | Create RFQ |
| POST | `/api/sales/quotations` | Create quotation |
| POST | `/api/sales/orders` | Create sales order |

Workflow: `RFQ -> Quotation -> Approval -> Sales Order -> Production Planning`.

## MES and clean room

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/work-orders/open` | Open work orders |
| POST | `/api/work-orders` | Create work order with 10 operations |
| POST | `/api/work-orders/operations/complete` | Complete operation and update yield |
| POST | `/api/clean-room/readings` | Record temperature, humidity, particle count |
| POST | `/api/clean-room/access-logs` | Record clean-room access |

## QMS

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/quality/inspections` | Create incoming, in-process, final, shipping, or packaging inspection |
| POST | `/api/quality/ncrs` | Create non-conformance report |
| POST | `/api/quality/capas` | Create CAPA |

## Inventory, packaging, shipping

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/inventory/items` | Inventory item list |
| POST | `/api/inventory/transactions` | Receipt, issue, consumption, return, adjustment, transfer |
| POST | `/api/packaging/vacuum-records` | Vacuum packaging validation |
| GET | `/api/shipping/shipments` | Shipment list |
| POST | `/api/shipping/shipments` | Create shipment |

## Traceability and codes

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/traceability/{serialNumber}` | Complete genealogy for serial |
| GET | `/api/codes/barcode/{value}` | SVG barcode |
| GET | `/api/codes/qr/{value}` | SVG QR code |

## Dashboards

| Method | Endpoint |
| --- | --- |
| GET | `/api/dashboards/production` |
| GET | `/api/dashboards/quality` |
| GET | `/api/dashboards/inventory` |
| GET | `/api/dashboards/clean-room` |
| GET | `/api/dashboards/shipping` |
