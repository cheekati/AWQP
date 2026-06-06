INSERT INTO auth.Roles (Id, Name, NormalizedName)
SELECT NEWID(), v.Name, UPPER(v.Name)
FROM (VALUES
    ('Admin'),
    ('Production Manager'),
    ('Production Engineer'),
    ('Quality Engineer'),
    ('Clean Room Operator'),
    ('Warehouse Staff'),
    ('Sales Team'),
    ('Customer')
) v(Name)
WHERE NOT EXISTS (SELECT 1 FROM auth.Roles r WHERE r.NormalizedName = UPPER(v.Name));
GO

INSERT INTO erp.ProductCategories (Id, Code, Name, Segment)
SELECT NEWID(), Code, Name, Segment
FROM (VALUES
    ('SEM-QTZ', 'Semiconductor Quartz', 1),
    ('CSEM-QTZ', 'Compound Semiconductor Quartz', 2),
    ('OPT-QTZ', 'Optical Fiber Quartz', 3),
    ('SOL-QTZ', 'Solar Cell Quartz', 4)
) v(Code, Name, Segment)
WHERE NOT EXISTS (SELECT 1 FROM erp.ProductCategories pc WHERE pc.Code = v.Code);
GO

INSERT INTO erp.MaterialGrades (Id, Code, Name, MinimumPurityPercent, SupplierSpecification)
SELECT NEWID(), Code, Name, Purity, Spec
FROM (VALUES
    ('HPQ-9999', 'High Purity Quartz 99.99', 99.9900, 'Semiconductor standard high purity quartz'),
    ('HPQ-99999', 'Ultra High Purity Quartz 99.999', 99.9990, 'Ultra low metallic impurity quartz')
) v(Code, Name, Purity, Spec)
WHERE NOT EXISTS (SELECT 1 FROM erp.MaterialGrades mg WHERE mg.Code = v.Code);
GO

DECLARE @sem UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM erp.ProductCategories WHERE Code = 'SEM-QTZ');
DECLARE @csem UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM erp.ProductCategories WHERE Code = 'CSEM-QTZ');
DECLARE @opt UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM erp.ProductCategories WHERE Code = 'OPT-QTZ');
DECLARE @sol UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM erp.ProductCategories WHERE Code = 'SOL-QTZ');
DECLARE @grade UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM erp.MaterialGrades WHERE Code = 'HPQ-99999');

INSERT INTO erp.Products (Id, Code, Name, ProductCategoryId, MaterialGradeId, ProductFamily, DefaultUom)
SELECT NEWID(), Code, Name, CategoryId, @grade, Family, 'EA'
FROM (VALUES
    ('PROC-TUBE', 'Process Tube', @sem, 'Semiconductor'),
    ('OUTER-TUBE', 'Outer Tube', @sem, 'Semiconductor'),
    ('INNER-TUBE', 'Inner Tube', @sem, 'Semiconductor'),
    ('QTZ-BOAT', 'Quartz Boat', @sem, 'Semiconductor'),
    ('CHAMBER', 'Quartz Chamber', @sem, 'Semiconductor'),
    ('MOCVD-TUBE', 'MO-CVD Tube Reactor', @csem, 'Compound Semiconductor'),
    ('BOX-REACTOR', 'Box Type Reactor', @csem, 'Compound Semiconductor'),
    ('LARGE-TRAY', 'Large Tray', @csem, 'Compound Semiconductor'),
    ('FURNACE-TUBE', 'Quartz Furnace Tube', @opt, 'Optical Fiber'),
    ('MULTI-BURNER', 'Quartz Multi-Tube Burner', @opt, 'Optical Fiber'),
    ('SOLAR-TUBE', 'Solar Quartz Tube', @sol, 'Solar Cell'),
    ('SOLAR-BOAT', 'Solar Quartz Boat', @sol, 'Solar Cell')
) v(Code, Name, CategoryId, Family)
WHERE NOT EXISTS (SELECT 1 FROM erp.Products p WHERE p.Code = v.Code);
GO

INSERT INTO erp.CleanRoomAreas (Id, Code, Name, IsoClass, TemperatureLowerC, TemperatureUpperC, HumidityLowerPercent, HumidityUpperPercent, ParticleLimitPerCubicFoot)
SELECT NEWID(), 'CR-1000-FIN', 'Final Operations Clean Room', 'Class 1000', 20, 24, 40, 55, 1000
WHERE NOT EXISTS (SELECT 1 FROM erp.CleanRoomAreas WHERE Code = 'CR-1000-FIN');
GO

INSERT INTO erp.Machines (Id, Code, Name, WorkCenter, RatedCapacityPerHour, IsCleanRoomQualified)
SELECT NEWID(), Code, Name, WorkCenter, Capacity, CleanRoom
FROM (VALUES
    ('CUT-01', 'Quartz Cutting Saw', 'Raw Material Preparation', 10.0, 0),
    ('LATHE-01', 'Quartz Precision Lathe', 'Forming', 4.0, 0),
    ('FIRE-01', 'Quartz Firing Furnace', 'Firing', 8.0, 0),
    ('CLEAN-01', 'Final Cleaning Station', 'Clean Room', 12.0, 1),
    ('VAC-01', 'Vacuum Packaging Station', 'Clean Room', 12.0, 1)
) v(Code, Name, WorkCenter, Capacity, CleanRoom)
WHERE NOT EXISTS (SELECT 1 FROM erp.Machines m WHERE m.Code = v.Code);
GO

INSERT INTO erp.Warehouses (Id, Code, Name, SiteCode)
SELECT NEWID(), 'MAIN', 'Main Warehouse', 'HQ'
WHERE NOT EXISTS (SELECT 1 FROM erp.Warehouses WHERE Code = 'MAIN');
GO

DECLARE @warehouse UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM erp.Warehouses WHERE Code = 'MAIN');
INSERT INTO erp.WarehouseLocations (Id, WarehouseId, Code, Name, Zone, Bin, IsCleanRoomStorage)
SELECT NEWID(), @warehouse, Code, Name, Zone, Bin, CleanRoomStorage
FROM (VALUES
    ('RAW-A1', 'Raw Material Bin A1', 'RAW', 'A1', 0),
    ('WIP-FORM', 'Forming WIP', 'WIP', 'FORM', 0),
    ('FG-CR1', 'Clean Room Finished Goods', 'FG', 'CR1', 1)
) v(Code, Name, Zone, Bin, CleanRoomStorage)
WHERE NOT EXISTS (SELECT 1 FROM erp.WarehouseLocations wl WHERE wl.Code = v.Code);
GO
