INSERT INTO erp.ProductCategories (Id, Name, IndustrySegment, CreatedBy) VALUES
('10000000-0000-0000-0000-000000000001', 'Process Tube', 1, 'seed'),
('10000000-0000-0000-0000-000000000002', 'Quartz Boat', 1, 'seed'),
('10000000-0000-0000-0000-000000000003', 'MO-CVD Tube Reactor', 2, 'seed'),
('10000000-0000-0000-0000-000000000004', 'Quartz Furnace Tube', 3, 'seed'),
('10000000-0000-0000-0000-000000000005', 'Solar Quartz Tube', 4, 'seed');

INSERT INTO erp.Products (Id, ProductCode, Name, ProductCategoryId, MaterialGrade, OuterDiameterMm, InnerDiameterMm, LengthMm, WallThicknessMm, CreatedBy) VALUES
('20000000-0000-0000-0000-000000000001', 'QPT-300-2200', 'Semiconductor Process Tube 300mm', '10000000-0000-0000-0000-000000000001', 'HPQ-99.9999', 300, 286, 2200, 7, 'seed'),
('20000000-0000-0000-0000-000000000002', 'QQB-25W', '25 Wafer Quartz Boat', '10000000-0000-0000-0000-000000000002', 'HPQ-99.9999', 180, 0, 420, 5, 'seed');

INSERT INTO cleanroom.CleanRooms (Id, CleanRoomCode, Name, CleanRoomClass, TemperatureMinC, TemperatureMaxC, HumidityMinPercent, HumidityMaxPercent, ParticleCountMax, CreatedBy) VALUES
('30000000-0000-0000-0000-000000000001', 'CR-1000-FINAL', 'Final Operations Clean Room', 'Class 1000', 20, 24, 40, 60, 1000, 'seed');

INSERT INTO mes.Machines (Id, MachineCode, Name, WorkCenter, IsCleanRoomQualified, CreatedBy) VALUES
('40000000-0000-0000-0000-000000000001', 'FURN-01', 'Quartz Firing Furnace 01', 'Firing', 0, 'seed'),
('40000000-0000-0000-0000-000000000002', 'VAC-01', 'Vacuum Packaging Station 01', 'CleanRoomPackaging', 1, 'seed');
GO
