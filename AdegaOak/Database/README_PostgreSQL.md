# 🎯 Banco de Dados PostgreSQL - AdegaOak

## ⚠️ IMPORTANTE: Você NÃO precisa executar scripts manualmente!

O **Entity Framework** cria tudo automaticamente através das **migrations**.

## 🚀 Método Recomendado (Automático)

### Opção 1: Deixar o Railway criar automaticamente (RECOMENDADO)

1. Configure a variável `DATABASE_URL` no Railway com sua connection string do Supabase
2. Faça deploy (`git push`)
3. O Railway vai executar as migrations automaticamente na inicialização
4. ✅ Pronto! Tabelas criadas e usuário admin inserido

### Opção 2: Criar localmente antes do deploy

```powershell
# 1. Configure a connection string
$env:DATABASE_URL="postgresql://postgres.xxxxx:password@aws-0-us-east-1.pooler.supabase.com:6543/postgres"

# 2. Navegue até o diretório
cd AdegaOak

# 3. Gere a migration (se ainda não existe)
dotnet ef migrations add PostgreSQLMigration --project AdegaOak.Data --startup-project AdegaOak.Api

# 4. Aplique a migration (cria as tabelas)
dotnet ef database update --project AdegaOak.Data --startup-project AdegaOak.Api
```

Isso vai:
- ✅ Criar todas as 8 tabelas
- ✅ Criar todos os índices
- ✅ Criar todas as foreign keys
- ✅ Inserir o usuário admin (username: `admin`, password: `admin`)
- ✅ Inserir a configuração de saldo inicial

## 📊 Estrutura do Banco

### Tabelas Criadas:

1. **Usuarios** - Usuários do sistema
2. **Produtos** - Catálogo de produtos
3. **Movimentacoes** - Entradas e saídas de estoque
4. **Despesas** - Controle de despesas
5. **Combos** - Combos de produtos
6. **ComboComposicoes** - Produtos que compõem cada combo
7. **ComboVendas** - Vendas de combos
8. **SaldoConfigs** - Configuração de saldo/capital

### Dados Iniciais (Seed):

- **Usuário Admin**:
  - Username: `admin`
  - Password: `admin`
  - Role: `admin`
  - Ativo: `true`

- **Saldo Inicial**:
  - Capital Admin: `0.00`

## 🔍 Como Verificar se Funcionou

### No Railway (Logs):

Procure por estas mensagens:
```
[DATABASE] Using PostgreSQL (Supabase)
[DATABASE] Connection: postgresql://postgres.xxxxx:***@...
[DATABASE] Applying migrations...
[DATABASE] ✅ Migrations applied successfully
[DATABASE] Connection verified: True
[DATABASE] ✅ Users in database: 1
```

### No Supabase (Dashboard):

1. Acesse seu projeto no Supabase
2. Vá em **Table Editor**
3. Você deve ver 8 tabelas criadas
4. Abra a tabela `Usuarios` e veja o usuário admin

### No pgAdmin (Opcional):

1. Conecte ao seu banco Supabase
2. Expanda **Schemas** → **public** → **Tables**
3. Você deve ver as 8 tabelas

## 📝 Script SQL Manual (Apenas para Referência)

Se você **realmente** quiser criar manualmente (não recomendado), use o arquivo:
- `PostgreSQL_Setup.sql`

Mas lembre-se:
- ⚠️ Você vai precisar gerenciar migrations manualmente depois
- ⚠️ Pode causar conflitos com o Entity Framework
- ⚠️ Não é a forma recomendada

## 🔧 Troubleshooting

### Erro: "relation already exists"
**Causa:** Tabelas já foram criadas

**Solução:** Não precisa fazer nada! As tabelas já existem.

### Erro: "No users found in database"
**Causa:** Migration seed não foi executada

**Solução:**
```sql
-- Execute no pgAdmin ou Supabase SQL Editor
INSERT INTO "Usuarios" ("Nome", "Username", "PasswordHash", "Role", "Ativo", "CriadoEm")
VALUES ('Administrador', 'admin', '$2a$11$8vZ7YQfyVZxH5L3qN9X8/.rKZJ5mHJxGxN8fQXqZ5vZ7YQfyVZxH5', 'admin', true, '2024-01-01 00:00:00');
```

### Erro: "DATABASE_URL environment variable is required"
**Causa:** Variável não configurada

**Solução:** Configure `DATABASE_URL` no Railway ou localmente

## 🎉 Resumo

✅ **Use migrations** (automático)  
❌ **Não execute scripts SQL manualmente** (a menos que seja necessário)

O Entity Framework cuida de tudo! 🚀
