# 🎲 PhotoRoulette - Backend

Ovaj projekt predstavlja pozadinski (backend) sustav za Android aplikaciju **PhotoRoulette**, izrađen u .NET tehnologiji. Igra omogućuje više igrača da sudjeluju u zabavnim rundama gdje pokušavaju pogoditi tko je poslao koju fotografiju. Backend koristi **SignalR** za komunikaciju u stvarnom vremenu i sinkronizaciju stanja igre među povezanim klijentima. Omogućeno je paralelno pokretanje više zasebnih igara (lobbyja), upravljanje rundama, automatsko bodovanje te kasnije pregledavanje rezultata. Igrači se mogu pridružiti unosom naziva postojećeg lobbyja, a administratorima je omogućeno i naknadno uređivanje podataka.

Sustav podržava prijavu putem klasičnog sustava autentifikacije te putem **OAuth autentifikacije s Google računom**. Postoje tri razine pristupa: neprijavljeni korisnici, prijavljeni korisnici s **Manager** rolom i **Admin** korisnici. Svaka razina ima različita prava pristupa, prikazana na slici u nastavku.

Backend je hostan na privatnom **Ubuntu serveru**, kojem se pristupa putem domene `http://tockanetfotorulet.ddns.net`, postavljene pomoću **No-IP** servisa. Za obradu vanjskih zahtjeva koristi se **Apache kao reverse proxy**, koji preusmjerava HTTP promet prema .NET aplikaciji.

---

## 🔧 Funkcionalnosti

- ✅ Upravljanje višestrukim igrama u stvarnom vremenu putem **SignalR WebSocketa**.
- 🎮 Mogućnost **pokretanja igre**, pridruživanja igrača, automatsko odigravanje rundi te **automatsko izračunavanje rezultata**.
- 🖼️ Pregled svih **odigranih igara** i njihovih rezultata nakon završetka.
- 📝 **Naknadna izmjena podataka** nakon završetka igre — npr. ispravak pogrešaka.
- ✉️ **Pozivanje igrača u određeni lobby** putem pozivnica ili poveznica.
- 🔄 Dostupan **REST API za dohvat podataka** o igrama, korisnicima i rezultatima radi integracije s Android frontendom i administracijskih potreba.

---

## 🚀 Tehnologije

- .NET 8 (ASP.NET Core)
- SignalR (real-time komunikacija)
- OAuth 2.0 (Google prijava)
- REST API
- Apache reverse proxy
- Ubuntu Server (self-hosted deployment)
- No-IP (dinamička domena)

---