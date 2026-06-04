-- ============================================================================
--  Orbital Greenhouse - Dados de exemplo para simulação de uso
--  Execute APÓS 01_create_tables.sql.
--
--  Observação: se o schema foi criado via EF Core (dotnet ef database update),
--  as 8 métricas em OGH_METRIC_TYPES já são inseridas pelo seed do EF. Nesse
--  caso, pule a seção 1 deste script para evitar violação de UX_METRIC_CODE.
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 1) Catálogo de métricas ambientais
-- ---------------------------------------------------------------------------
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('TEMPERATURE',  'Air Temperature',      'C',         'Temperatura do ar no módulo de cultivo.',          18,   26);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('HUMIDITY',     'Relative Humidity',    '%',         'Umidade relativa do ar.',                          50,   80);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('CO2',          'Carbon Dioxide',       'ppm',       'Concentração de CO2 para fotossíntese.',          400, 1200);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('O2',           'Oxygen',               '%',         'Concentração de oxigênio na atmosfera da cabine.', 19,   23);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('LUMINOSITY',   'Photosynthetic Light', 'umol/m2/s', 'Radiação fotossinteticamente ativa.',             200,  800);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('SOIL_MOISTURE','Substrate Moisture',   '%',         'Umidade do substrato de cultivo.',                 40,   70);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('PH',           'Nutrient Solution pH', 'pH',        'Acidez da solução nutritiva hidropônica.',        5.5,  6.5);
INSERT INTO OGH_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('EC',           'Nutrient Conductivity','mS/cm',     'Condutividade elétrica (força nutritiva).',       1.2,  2.4);

-- ---------------------------------------------------------------------------
-- 2) Regiões (módulos de cultivo)
-- ---------------------------------------------------------------------------
INSERT INTO OGH_REGIONS (Code, Name, ModuleType, OrbitalSegment, Description) VALUES ('BAY-A1', 'Hydroponic Bay A1', 'Hydroponic', 'Deck 3 / Node 2', 'Baia hidropônica de folhosas.');
INSERT INTO OGH_REGIONS (Code, Name, ModuleType, OrbitalSegment, Description) VALUES ('BAY-B2', 'Aeroponic Bay B2',  'Aeroponic',  'Deck 4 / Node 1', 'Baia aeropônica de tubérculos.');
INSERT INTO OGH_REGIONS (Code, Name, ModuleType, OrbitalSegment, Description) VALUES ('BAY-C3', 'Soil Module C3',    'Soil',       'Deck 2 / Node 3', 'Módulo de solo para frutíferas.');

-- ---------------------------------------------------------------------------
-- 3) Dispositivos / sensores (vinculados às regiões pelo Id - ajuste se necessário)
-- ---------------------------------------------------------------------------
INSERT INTO OGH_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-A1-01', 'MultiSensor A1-01', 'MultiSensor', 'Active', '1.4.2', (SELECT Id FROM OGH_REGIONS WHERE Code = 'BAY-A1'));
INSERT INTO OGH_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-A1-02', 'CO2 Probe A1-02',   'CO2 Probe',   'Active', '1.4.2', (SELECT Id FROM OGH_REGIONS WHERE Code = 'BAY-A1'));
INSERT INTO OGH_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-B2-01', 'MultiSensor B2-01', 'MultiSensor', 'Active', '1.5.0', (SELECT Id FROM OGH_REGIONS WHERE Code = 'BAY-B2'));
INSERT INTO OGH_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-C3-01', 'Soil Probe C3-01',  'Soil Probe',  'Maintenance', '1.3.7', (SELECT Id FROM OGH_REGIONS WHERE Code = 'BAY-C3'));

-- ---------------------------------------------------------------------------
-- 4) Regras de alerta (limiares de salubridade)
-- ---------------------------------------------------------------------------
INSERT INTO OGH_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('CO2 alto (global)',          'CO2 acima do ideal em qualquer região.', (SELECT Id FROM OGH_METRIC_TYPES WHERE Code='CO2'),          NULL, NULL, 1200, 'Warning',  1);
INSERT INTO OGH_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('O2 crítico (global)',        'Oxigênio fora da faixa segura.',          (SELECT Id FROM OGH_METRIC_TYPES WHERE Code='O2'),           NULL, 19,   23,  'Critical', 1);
INSERT INTO OGH_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('Temperatura fora de faixa',  'Temperatura do ar fora do ideal.',        (SELECT Id FROM OGH_METRIC_TYPES WHERE Code='TEMPERATURE'),  NULL, 18,   26,  'Warning',  1);
INSERT INTO OGH_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('Umidade baixa na BAY-A1',    'Umidade abaixo de 50% na baia A1.',       (SELECT Id FROM OGH_METRIC_TYPES WHERE Code='HUMIDITY'),     (SELECT Id FROM OGH_REGIONS WHERE Code='BAY-A1'), 50, NULL, 'Warning', 1);

-- Usuário: o hash de senha é gerado pela API (ASP.NET Core Identity PasswordHasher).
-- Crie o operador via POST /api/v1/auth/register em vez de inserir aqui.

COMMIT;
