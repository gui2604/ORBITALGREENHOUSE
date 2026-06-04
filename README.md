# Orbital Greenhouse API

API RESTful em **ASP.NET Core 9 (C#)** para **monitoramento ambiental e gestão de
alertas** na **produção de alimentos no espaço**. Dispositivos/sensores de campo
coletam métricas ambientais (umidade, luminosidade, oxigênio, gás carbônico, etc.),
transmitem essas leituras como **arquivos JSON**, e o servidor avalia regras de
alerta configuráveis para identificar a **salubridade** de cada região de cultivo
e gerar relatórios.

> Projeto desenvolvido como entrega de **Modelo de Banco de Dados** + **Implementação
> de Serviços (API)**, usando **Oracle** como banco relacional.

---

## Sobre o domínio

Uma estufa orbital é dividida em **regiões** (baias/módulos de cultivo). Cada região
contém **dispositivos** (sensores) que periodicamente enviam **leituras**, e cada
leitura agrupa várias **medições** de **tipos de métrica** do catálogo. **Regras de
alerta** definem limiares (mínimo/máximo) por métrica — globais ou por região —, e
quando uma medição viola um limiar, um **alerta** é gerado automaticamente com a
severidade configurada. Operadores autenticados (via **JWT**) consultam, reconhecem
e resolvem os alertas.

---

## Arquitetura em camadas

```
Controller  ->  Service  ->  Repository  ->  DbContext (EF Core / Oracle)
   (HTTP)       (regras)     (acesso a dados)     (persistência)
```

- **Controllers** — expõem os endpoints REST e fazem binding/validação de DTOs.
- **Services** — concentram as regras de negócio (ingestão, avaliação de alertas, relatórios, auth).
- **Repositories** — abstraem o acesso a dados (genérico `IRepository<T>` + repositórios específicos).
- **DbContext** — mapeamento EF Core para Oracle, com nomes de objetos curtos e portáveis.

```
OrbitalGreenhouse/
├── OrbitalGreenhouse.sln
├── OrbitalGreenhouse.Api/
│   ├── Controllers/        # Regions, Devices, MetricTypes, AlertRules, Alerts, Ingestion, Readings, Reports, Auth, Health, Root
│   ├── Services/           # *Service.cs + Mappers
│   ├── Repositories/       # IRepository<T> + repositórios específicos
│   ├── Data/               # ApplicationDbContext + DesignTimeDbContextFactory
│   ├── Models/             # Entidades + Enums
│   ├── Dtos/               # DTOs de entrada/saída
│   ├── Exceptions/         # Exceções de domínio + GlobalExceptionHandler
│   ├── Migrations/         # Migrations EF Core (Oracle)
│   ├── SampleData/         # Arquivos JSON de sensores para simulação
│   └── appsettings.json
├── db/                     # Scripts SQL (DDL, seed, consultas) + script EF idempotente
└── docs/                   # Diagrama ER + coleção Postman
```

---

## Modelo de dados

8 entidades / tabelas (prefixo `OGH_`):

| Tabela | Descrição |
|--------|-----------|
| `OGH_REGIONS` | Regiões/módulos de cultivo |
| `OGH_METRIC_TYPES` | Catálogo de métricas ambientais (unidade + faixa nominal) |
| `OGH_DEVICES` | Sensores de campo vinculados a uma região |
| `OGH_SENSOR_READINGS` | Evento de coleta (1 arquivo JSON) |
| `OGH_MEASUREMENTS` | Cada valor de métrica de uma leitura |
| `OGH_ALERT_RULES` | Limiares configuráveis (min/max) por métrica/região |
| `OGH_ALERTS` | Alertas gerados (automáticos ou manuais) com ciclo de vida |
| `OGH_USERS` | Operadores autenticados via JWT |

O **diagrama ER** (Mermaid) está em [`docs/ER_DIAGRAM.md`](docs/ER_DIAGRAM.md).

---

## Tecnologias

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9 + **Oracle** (`Oracle.EntityFrameworkCore`)
- Autenticação **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swagger / OpenAPI** (Swashbuckle)
- Arquitetura em camadas (Controller / Service / Repository)

---

## Configuração do banco Oracle

Servidor FIAP: `oracle.fiap.com.br`, SID/service `ORCL`, usuário `RM97674` (alias TNS **Oracle_FIAP** no SQL Developer).

1. Copie o exemplo local (não versionado):

   ```bash
   copy OrbitalGreenhouse.Api\appsettings.Development.local.example.json OrbitalGreenhouse.Api\appsettings.Development.local.json
   ```

2. Edite `appsettings.Development.local.json` e informe sua senha:

```json
"ConnectionStrings": {
  "OracleDb": "User Id=RM97674;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL"
}
```

> Formato do `Data Source`: `HOST:PORTA/SERVICE_NAME` (equivalente ao TNS `Oracle_FIAP`).
> O arquivo `*.local.json` está no `.gitignore` — **não commite a senha**.
> Em produção, use `ConnectionStrings__OracleDb` como variável de ambiente.

---

## Docker

A API pode rodar em container (Oracle FIAP continua **externo** — o container só precisa de rede até `oracle.fiap.com.br`).

```bash
# 1. Criar .env a partir do exemplo e informar ORACLE_PASSWORD
copy .env.example .env

# 2. Subir a API (build + run)
docker compose up --build -d

# 3. Swagger
#    http://localhost:8080/swagger
```

Imagem publicada no **GitHub Container Registry**:

```bash
docker pull ghcr.io/gui2604/orbitalgreenhouse-api:latest
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ORACLE_PASSWORD=SUA_SENHA \
  -e ConnectionStrings__OracleDb="User Id=RM97674;Password=${ORACLE_PASSWORD};Data Source=oracle.fiap.com.br:1521/ORCL" \
  ghcr.io/gui2604/orbitalgreenhouse-api:latest
```

> Após o primeiro `docker push`, torne o pacote **público** em GitHub → seu perfil → **Packages** → `orbitalgreenhouse-api` → Package settings → Change visibility.

Comandos úteis:

```bash
docker compose logs -f api
docker compose down
```

Em **Development**, o container aplica migrations do EF na inicialização (mesmo comportamento do `dotnet run` local).

---

## Como executar

```bash
# 1. Restaurar pacotes
dotnet restore

# 2. Aplicar o schema no Oracle (cria tabelas + seed das métricas)
dotnet ef database update --project OrbitalGreenhouse.Api

# 3. Rodar a API
dotnet run --project OrbitalGreenhouse.Api

# 4. Abrir o Swagger
#    https://localhost:7118/swagger
```

Alternativa sem EF: aplicar manualmente os scripts em [`db/`](db/) no Oracle SQL Developer.

### Fluxo de teste no Swagger
1. `POST /api/v1/auth/register` → cria um operador e retorna um token JWT.
2. Clique em **Authorize** e informe `Bearer {token}`.
3. Cadastre regiões, dispositivos e regras de alerta.
4. Envie leituras em `POST /api/v1/ingestion/readings` (ou faça upload de um arquivo de `SampleData/`).
5. Consulte alertas e relatórios.

---

## Endpoints

### Públicos
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/auth/register` | Cria operador e retorna JWT |
| POST | `/api/v1/auth/login` | Autentica e retorna JWT |
| GET | `/api/healthcheck` | Liveness básico |
| GET | `/` | Informações da API |

### Protegidos (requer JWT)
| Recurso | Endpoints |
|---------|-----------|
| **Regions** | `GET /api/v1/regions` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| **Devices** | `GET /api/v1/devices` · `GET /{id}` · `GET /by-region/{regionId}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| **Metric Types** | `GET /api/v1/metric-types` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| **Alert Rules** | `GET /api/v1/alert-rules` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| **Ingestion** | `POST /api/v1/ingestion/readings` · `POST /readings/batch` · `POST /upload` (arquivo JSON) |
| **Alerts** | `GET /api/v1/alerts` (filtros) · `GET /{id}` · `POST` (manual) · `PUT /{id}/status` · `DELETE /{id}` |
| **Readings** | `GET /api/v1/readings/{id}` · `GET /by-device/{deviceId}` |
| **Reports** | `GET /api/v1/reports/region-health` · `/{regionId}` · `/alerts-summary` |
| **Health** | `GET /api/healthcheck/full` (verifica conexão Oracle) |

Coleção Postman pronta em [`docs/OrbitalGreenhouse.postman_collection.json`](docs/OrbitalGreenhouse.postman_collection.json).

---

## Simulação de entrada de métricas (arquivos JSON)

Cada sensor envia um JSON. Exemplos em [`OrbitalGreenhouse.Api/SampleData/`](OrbitalGreenhouse.Api/SampleData/):

```json
{
  "deviceIdentifier": "SENSOR-A1-02",
  "collectedAt": "2026-06-04T12:05:00Z",
  "measurements": [
    { "metricCode": "CO2", "value": 1850 },
    { "metricCode": "O2", "value": 17.8 }
  ]
}
```

Ao ingerir, a API: persiste a leitura e as medições, atualiza o `LastSeenAt` do
dispositivo, **avalia as regras de alerta ativas** para a métrica/região e cria os
alertas correspondentes — retornando quantos alertas foram disparados.

---

## Scripts SQL (pasta `db/`)

| Arquivo | Conteúdo |
|---------|----------|
| `01_create_tables.sql` | DDL limpo: `CREATE TABLE`, PKs, FKs e índices |
| `02_seed_data.sql` | Dados de exemplo (métricas, regiões, dispositivos, regras) |
| `03_sample_queries.sql` | Consultas de simulação (inventário, salubridade, relatórios) |
| `99_ef_idempotent_oracle.sql` | Script gerado pelo EF Core (aplicação idempotente) |

---

## Equipe / Disciplina

Projeto acadêmico — entrega de **Modelo de Banco de Dados** e **Implementação dos
Serviços (API)**. Contexto: produção de alimentos no espaço (*Orbital Greenhouse*).
