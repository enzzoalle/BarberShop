# Resumo da Arquitetura e Estrutura do Projeto: BarberShop

## Stack Tecnológica
- **Versão do .NET:** .NET 10.0 (`net10.0`)
- **Banco de Dados:** PostgreSQL (via `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **ORM:** Entity Framework Core 10.0.6
- **Front-end Web:** ASP.NET Core Razor Pages (pacote `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`)
- **API:** ASP.NET Core Web API com suporte a OpenAPI.

## Arquitetura e Projetos (`BarberShop.sln`)

A solução está estruturada em camadas, inspirada na *Clean Architecture* e em *N-Tier Design*, dividida nos seguintes projetos:

### 1. `App.Domain` (Core / Domínio)
- **O que é:** O coração do sistema. Contém as regras de negócio puras, entidades principais, DTOs, Enums e as interfaces (ex: contratos de repositórios).
- **Dependências:** Nenhuma (zero dependências externas ou de outros projetos).

### 2. `App.Persistence` (Infraestrutura de Dados)
- **O que é:** Responsável por interagir com o banco de dados. Implementa as interfaces do domínio usando Entity Framework Core. Contém o `AppDbContext`, implementações de repositório (`RepositoryBase.cs`) e as `Migrations`.
- **Dependências:** Referencia `App.Domain`.
- **Pacotes Principais:** `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`.

### 3. `App.Common` (Cross-Cutting / Utils)
- **O que é:** Projeto para utilitários transversais e funções compartilhadas por toda a aplicação (ex: `Criptografia.cs`, `TextoHelper.cs`).
- **Dependências:** Nenhuma.

### 4. `App.Application` (Camada de Aplicação / Casos de Uso)
- **O que é:** Contém a lógica de orquestração e os serviços da aplicação (`Services`). Intermedia as chamadas entre a API/Interface do usuário e o Domínio/Persistência.
- **Dependências atuais:** Referencia `App.Domain`, `App.Common` e `App.Persistence`. *(Atenção para as inconsistências arquiteturais relatadas no chat)*.

### 5. `App.Api` (Interface Externa / API REST)
- **O que é:** Expõe os serviços da aplicação via endpoints HTTP (REST). Usa controladores (`Controllers`) e está configurado com OpenAPI.
- **Dependências:** Referencia `App.Application` e `App.Persistence`.

### 6. `App.Web` (Interface de Usuário)
- **O que é:** Aplicação Web utilizando Razor Pages (contém pastas `Pages` e `wwwroot`).
- **Dependências atuais:** Atualmente não possui referência de projeto local para a Application.

## Regras de Negócio e Funcionalidades (Domain/Application)

O sistema BarberShop é uma plataforma para gestão e agendamento de barbearias. A seguir, os principais fluxos técnicos de negócio implementados nos `Services` da `App.Application`:

### 1. Gestão de Agendamentos (`AgendamentosService.cs`)
- **Cálculo de Disponibilidade:** Verifica a duração do serviço escolhido e calcula os horários disponíveis em intervalos de 30 minutos, respeitando:
  - O horário de expediente (abertura e fechamento).
  - Os dias de funcionamento (Ex: `AteSabado`, `DiasUteis`).
  - Feriados e folgas cadastradas.
  - Conflitos com outros agendamentos existentes no mesmo dia.
- **Tipos de Agendamento:**
  - **Solicitação do Cliente:** Entra com o status `Pendente`.
  - **Inclusão Manual (Admin/Barbeiro):** Entra automaticamente com o status `Aprovado`.
- **Comunicação:** Ao aprovar uma solicitação, o sistema gera um link dinâmico da API do WhatsApp (`https://wa.me/...`) com uma mensagem pré-formatada para notificar o cliente.
- **Gestão de Clientes:** O agendamento busca um cliente pelo telefone ou cria um novo registro caso não exista na base.

### 2. Configurações da Barbearia (`ParametrosService.cs`)
- **Expediente:** Define globalmente os horários de abertura/fechamento, contatos (telefone principal/secundário), localização e os dias de funcionamento gerais da barbearia.
- **Folgas e Feriados:** Mantém uma lista de datas (`FolgasFeriados`) que o sistema usa para bloquear automaticamente a agenda naqueles dias, sincronizando inserções e exclusões de datas.

### 3. Catálogo de Serviços (`ServicosService.cs`)
- **Cadastro:** Gerencia o portfólio oferecido (ex: Corte, Barba).
- **Regras:** Todo serviço possui obrigatoriamente uma `Duracao` (usada estritamente como bloco de tempo pelo cálculo da agenda) e um `Valor`.
- **Soft Delete:** Serviços podem ser ativados/inativados (`AlterarStatus`) em vez de excluídos fisicamente, mantendo a integridade dos históricos de agendamentos passados.

### 4. Controle de Acesso e Usuários (`UsuariosService.cs`)
- **Autenticação:** O login de administradores/barbeiros compara o hash SHA-512 da senha fornecida com o banco.
- **Validações de Unicidade:** Impede ativamente o cadastro de usuários duplicados (mesmo nome ou mesmo número de telefone).
- **Criptografia:** Senhas nunca são trafegadas ou salvas em texto limpo, sendo parseadas pela classe utilitária `Criptografia.GeraHash` (App.Common).
