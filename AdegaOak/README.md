# Adega Oak - Sistema de Gestão

Sistema completo de gestão para Adega Oak com backend em ASP.NET Core e frontend em Next.js 14.

## 🚀 Tecnologias

### Backend
- **ASP.NET Core 9.0** - Web API
- **Entity Framework Core** - ORM
- **SQLite** - Banco de dados local
- **JWT** - Autenticação
- **Swagger** - Documentação da API

### Frontend
- **Next.js 14** - Framework React com App Router
- **TypeScript** - Tipagem estática
- **Tailwind CSS** - Estilização
- **Zustand** - Gerenciamento de estado
- **Axios** - Cliente HTTP
- **React Query** - Cache e sincronização de dados
- **Recharts** - Gráficos e visualizações

## 📋 Pré-requisitos

- **.NET SDK 9.0+** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **npm ou yarn** - Gerenciador de pacotes

## 🏗️ Arquitetura

```
AdegaOak/
├── AdegaOak.Api/          # Controllers, Program.cs, configurações
├── AdegaOak.Data/         # DbContext, Migrations, Repositories
├── AdegaOak.Models/       # Entities, DTOs, Enums
├── AdegaOak.Services/     # Interfaces, Services, Use Cases
├── frontend/              # Next.js App
├── scripts/               # Scripts de execução
└── docs/                  # Documentação adicional
```

## 🎯 Funcionalidades

### Módulos Principais

1. **Dashboard** (Admin)
   - Visão geral de receitas e despesas
   - Top 10 produtos mais vendidos
   - Alertas de estoque baixo
   - Gráficos de vendas por período
   - Vendas por usuário

2. **Estoque**
   - Cadastro de produtos
   - Controle de quantidade em tempo real
   - Alertas de estoque mínimo
   - Gestão de preços (varejo, caixa, atacado)

3. **Movimentações**
   - Registro de entradas e saídas
   - Filtros por tipo, período, usuário
   - Histórico completo
   - Relatórios de movimentação

4. **Despesas** (Admin)
   - 14 tipos de despesas
   - Controle de pagamento
   - Despesas vinculadas a produtos
   - Relatórios por período

5. **Combos**
   - Criação de combos e copões
   - Composição de produtos
   - Controle de débito de estoque
   - Vendas de combos

6. **Usuários** (Admin)
   - Cadastro de funcionários
   - Controle de permissões (Admin/Funcionário)
   - Cada venda vinculada ao usuário

## 🚀 Como Executar Localmente

### Opção 1: Script Automático (Recomendado)

1. Navegue até a pasta `scripts`
2. Execute `run-all.bat`
3. Aguarde a inicialização (backend e frontend abrem em janelas separadas)
4. Acesse http://localhost:3000

### Opção 2: Manual

#### Backend

```bash
cd AdegaOak/AdegaOak.Api
dotnet restore
dotnet ef database update --project ../AdegaOak.Data/AdegaOak.Data.csproj
dotnet run --urls "http://localhost:5000"
```

#### Frontend

```bash
cd AdegaOak/frontend
npm install
npm run dev
```

## 🔐 Credenciais Padrão

- **Usuário:** admin
- **Senha:** Admin@2024

## 📡 Endpoints da API

### Autenticação
- `POST /api/auth/login` - Login
- `GET /api/auth/usuarios` - Listar usuários (Admin)
- `POST /api/auth/usuarios` - Criar usuário (Admin)

### Produtos
- `GET /api/produtos` - Listar produtos
- `POST /api/produtos` - Criar produto (Admin)
- `PUT /api/produtos/{id}/precos` - Atualizar preços (Admin)

### Movimentações
- `GET /api/movimentacoes` - Listar movimentações
- `POST /api/movimentacoes` - Criar movimentação
- `POST /api/movimentacoes/filtrar` - Filtrar movimentações

### Despesas
- `GET /api/despesas` - Listar despesas
- `POST /api/despesas` - Criar despesa (Admin)
- `PATCH /api/despesas/{id}/pagar` - Marcar como paga (Admin)

### Combos
- `GET /api/combos` - Listar combos
- `POST /api/combos` - Criar combo (Admin)
- `POST /api/combos/vendas` - Vender combo

### Dashboard
- `GET /api/dashboard/saldo` - Obter saldo
- `POST /api/dashboard` - Obter dashboard completo (Admin)

**Documentação completa:** http://localhost:5000/swagger

## 🌐 Deploy em Produção

### Backend (Azure App Service / AWS / VPS)

1. **Configurar banco de dados:**
   - Para produção, recomenda-se PostgreSQL ou SQL Server
   - Atualizar `appsettings.Production.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=seu-servidor;Database=adegaoak;..."
     }
   }
   ```

2. **Publicar aplicação:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

3. **Configurar variáveis de ambiente:**
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `Jwt__Key` - Chave secreta forte
   - `ConnectionStrings__DefaultConnection` - String de conexão

4. **Deploy:**
   - Azure: `az webapp up`
   - Docker: Criar Dockerfile e imagem
   - VPS: Copiar pasta `publish` e configurar Nginx/Apache como reverse proxy

### Frontend (Vercel / Netlify / Azure Static Web Apps)

1. **Configurar variável de ambiente:**
   ```
   NEXT_PUBLIC_API_URL=https://sua-api.com/api
   ```

2. **Build:**
   ```bash
   npm run build
   ```

3. **Deploy:**
   - **Vercel:** `vercel --prod`
   - **Netlify:** `netlify deploy --prod`
   - **Azure:** `az staticwebapp create`

### Exemplo Docker Compose

```yaml
version: '3.8'
services:
  backend:
    build: ./AdegaOak.Api
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=...
    
  frontend:
    build: ./frontend
    ports:
      - "3000:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://backend/api
```

## 🔧 Configurações Importantes

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=adegaoak.db"
  },
  "Jwt": {
    "Key": "SuaChaveSecretaAqui",
    "Issuer": "AdegaOakApi",
    "Audience": "AdegaOakFrontend"
  }
}
```

### Frontend (.env.local)

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

## 📊 Banco de Dados

O sistema usa SQLite por padrão para desenvolvimento local. O banco é criado automaticamente na primeira execução.

### Migrations

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration --project AdegaOak.Data

# Aplicar migrations
dotnet ef database update --project AdegaOak.Data

# Reverter migration
dotnet ef database update PreviousMigration --project AdegaOak.Data
```

## 🎨 Personalização

### Cores do Tema

Edite `frontend/tailwind.config.ts` para personalizar as cores:

```typescript
theme: {
  extend: {
    colors: {
      primary: '#92400e', // amber-900
      secondary: '#f59e0b', // amber-500
    }
  }
}
```

## 🐛 Troubleshooting

### Backend não inicia

- Verifique se a porta 5000 está disponível
- Confirme que o .NET SDK 9.0+ está instalado: `dotnet --version`
- Verifique os logs em `AdegaOak.Api/bin/Debug/net9.0/`

### Frontend não conecta à API

- Verifique se o backend está rodando
- Confirme a variável `NEXT_PUBLIC_API_URL` em `.env.local`
- Verifique o console do navegador para erros CORS

### Erro de autenticação

- Limpe o localStorage do navegador
- Verifique se a chave JWT está configurada corretamente
- Confirme que o token não expirou (validade: 7 dias)

## 📝 Licença

Este projeto é proprietário da Adega Oak.

## 👥 Suporte

Para suporte, entre em contato com a equipe de desenvolvimento.

---

**Desenvolvido com ❤️ para Adega Oak**
