# SearchHistoryService

Microservizio responsabile della registrazione e gestione della cronologia delle ricerche degli utenti all'interno dell'applicazione **Cocktail Débâcle**.

## 📦 Funzionalità principali

- Salvataggio delle ricerche effettuate dagli utenti (autenticati)
- Visualizzazione cronologia personale
- Statistiche personali aggregate
- Statistiche globali (solo admin)
- Filtri recenti e popolari
- Eliminazione cronologia

## 🔐 Autenticazione

Tutte le API richiedono un **JWT Bearer token** valido (incluso nel header `Authorization`).  
Solo le route `/global/*` richiedono il ruolo `admin`.

## 🔧 Variabili ambiente richieste

```json
"Jwt": {
  "Key": "your-secret-key",
  "Issuer": "CocktailDebacle",
  "Audience": "CocktailDebacleUsers"
}
```

## 📁 Rotte disponibili

### 👤 API Utente

| Metodo | Endpoint                                | Descrizione |
|--------|------------------------------------------|-------------|
| POST   | `/api/searchhistory`                    | Salva una ricerca con filtri |
| GET    | `/api/searchhistory/mine?limit=n`       | Ottiene la propria cronologia, con limite opzionale |
| GET    | `/api/searchhistory/mine/stats`         | Statistiche personali sui filtri usati |
| GET    | `/api/searchhistory/mine/cocktails`     | Ultimi 10 cocktail selezionati (`action == select`) |
| DELETE | `/api/searchhistory/delete`             | Elimina tutta la cronologia personale |
| DELETE | `/api/searchhistory/delete/{id}`        | Elimina una singola ricerca (solo se appartenente all'utente) |

### 📊 Statistiche generali

| Metodo | Endpoint                                | Descrizione |
|--------|------------------------------------------|-------------|
| GET    | `/api/searchhistory/popular-filters`    | Tutti i filtri più usati divisi per categoria |
| GET    | `/api/searchhistory/recent-filters`     | Ultimi 20 filtri usati (tendenze) |
| GET    | `/api/searchhistory/filter-summary`     | Riepilogo dei filterType più frequenti |
| GET    | `/api/searchhistory/popular-cocktails`  | Cocktail più cercati (filterType == "cocktail") |
| GET    | `/api/searchhistory/popular-glasses`    | Bicchieri più usati (filterType == "glass") |

### 🔐 Solo Admin

| Metodo | Endpoint                          | Descrizione |
|--------|------------------------------------|-------------|
| GET    | `/api/searchhistory/global/stats` | Statistiche globali (tutti gli utenti) |

## 💡 Esempi d'uso

- **Personalizzazione UX**: mostrare all'utente i suoi cocktail più selezionati recentemente
- **Suggerimenti automatici**: sfruttare i filtri più usati per popolare suggerimenti dinamici
- **Dashboard Admin**: analisi aggregata dei trend globali o anomalie nelle ricerche
- **Supporto a strategie di marketing**: individuare i cocktail e bicchieri più popolari per campagne mirate

## 🗃️ Database

- `SearchHistories`: ogni ricerca effettuata, legata all'utente
- `SearchFilters`: lista dei filtri (es. ingredienti, bicchieri) legati a una ricerca

## 📎 Note tecniche

- Backend in ASP.NET Core 8
- DB PostgreSQL
- JWT supportato
- Protezione cicli JSON con `ReferenceHandler.IgnoreCycles`
- Reverse proxy previsto: path gateway `/searchhistory/`

---

© 2025 - Cocktail Débâcle
