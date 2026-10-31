# 11 – Hosting options (deferred decision)

Hosting is decided in **Phase 6**, using real numbers (DB size, traffic, cold-start tolerance).
Everything ships as a Docker image, so any host works.
Prices below were checked in October 2026 and are estimates. Re-check before deciding.

## Frontend and backend

Two services. The **backend must be private** (internal-only ingress or private network).
Only the backend needs database and email access. The frontend needs persistent storage for cookie keys (Data Protection).

| Option | Cost | Notes |
|---|---|---|
| **Azure Container Apps** (Consumption) | Free grant: 180,000 vCPU-s, 360,000 GiB-s, 2M requests per month | Backend can use **internal ingress** in the same environment. Scale to zero → near €0, but cold starts can stack (frontend + backend). One always-on small replica each ≈ €10–30/month. Free grant is per subscription, shared by both apps. Managed identity possible for service auth. |
| Azure App Service | Free F1 is very limited; B1 is a fixed monthly cost | Simple, always on |
| VPS (e.g. Hetzner) + Docker | ≈ €4–8/month | Cheapest predictable option. Backend stays on the internal Docker network. You manage updates, TLS (Caddy), backups. |

Guests keep progress in the browser, so **scale to zero is viable**. With two services, cold starts can stack; consider keeping the frontend warm.

## OpenTelemetry Collector

- Small container on the private network (e.g. Grafana Alloy or the OpenTelemetry Collector).
- Only component with Grafana Cloud credentials and outbound telemetry access.
- Cost: one extra small container (scale to zero is not suitable; it must be up to receive telemetry). Expect a small always-on cost on Container Apps, or €0 extra on a VPS.

## PostgreSQL

| Option | Cost | Notes |
|---|---|---|
| Container on the same VPS | €0 extra | You handle backups |
| **Neon** Free | 100 CU-hours and 0.5 GB storage per project per month | Scales to zero after ~5 min idle; compute suspends at limits. Plenty for this app early on. |
| **Azure Database for PostgreSQL – Flexible Server** | Free 12 months with an Azure free account: 750 h B1MS, 32 GB storage, 32 GB backup | See below |
| Supabase Free | Small free instance | Includes extra services we don't need |

### Azure Flexible Server notes

- B1ms: 1 vCore, 2 GiB RAM.
- **Set storage to exactly 32 GB and disable auto-grow.** The free meter counts *provisioned* storage.
- Burstable tier is aimed at dev/test, no 24/7 support. Fine for a hobby app.
- **No scale to zero.** Stopping is possible but it auto-starts after 7 days.
- Built-in PgBouncer, backups kept 7–35 days.
- After the free year: roughly €15–20/month (B1ms + 32 GB).

## Email (Phase 5+)

Azure Communication Services, Brevo, Postmark or similar. Pick by free tier and deliverability.

## Example low-cost combinations

| Combination | Year 1 | After |
|---|---|---|
| Container Apps (scale to zero) + Neon Free | ≈ €0 | ≈ €0 |
| Container Apps + Azure Flexible Server | ≈ €0 | ≈ €15–20/month |
| VPS with app + Postgres containers | ≈ €4–8/month | same |
