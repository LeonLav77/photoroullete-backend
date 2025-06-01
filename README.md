# 🎲 PhotoRoulette - Backend

Ovaj projekt predstavlja pozadinski (backend) sustav za Android aplikaciju **PhotoRoulette**, izrađen u .NET tehnologiji. Igra omogućuje više igrača da sudjeluju u zabavnim rundama gdje pokušavaju pogoditi tko je poslao koju fotografiju. Backend koristi **SignalR** za komunikaciju u stvarnom vremenu i sinkronizaciju stanja igre među povezanim klijentima. Omogućeno je paralelno pokretanje više zasebnih igara (lobbyja), odigravanje rundi, automatsko bodovanje te kasnije pregledavanje rezultata. Igrači se mogu pridružiti unosom naziva postojećeg lobbyja, a administratorima je omogućeno i naknadno uređivanje podataka.

Sustav podržava prijavu putem klasičnog sustava autentifikacije te putem **OAuth autentifikacije s Google računom**. Postoje tri razine pristupa: neprijavljeni korisnici, prijavljeni korisnici s **Manager** rolom i **Admin** korisnici. Svaka razina ima različita prava pristupa.

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

## Kriteriji
Da li objekti imaju smisla (minimalno 4 entity framework klase, ne racunajuci User) → Answer, FcmToken, Game, Lobby, Player, Round

Da li tipovi podataka u objektima imaju smisla (datumi, nullable gdje treba, int vs string, ..) → npr u Game, id = int, code = string, createdAt DateTime

Da li su naznačene ispravne veze među objektima (1-N, N-N, nasljeđivanje) → 1 Game ima više Player i Rundi, Jedna Runda ima više Answera

Da li postoji kompletni izbornik u aplikaciji? → ?

Postoji li custom ruta definirana u RouteConfig-u? (recimo, /kompanije/pregled i sl.) → "api/game/import/{gameCode:regex(^[a-zA-Z0-9_-]+$)}"

Da li postoji ruta definirana atributima/anotacijama? → [Authorize(Roles = "Manager,Admin")]

Da li je kroz aplikaciju moguće izmjeniti podatke za barem 2 entiteta (ovisno o poslovnim pravilima) → Mogu se brisati runde, mjenjati scoreovi, mjenjati Excitement level na Gameu

Postoji li validacija (server side) → Role Validation, Update Game Validation

Drop down liste (unos vezanih vrijednosti obvezno preko drop down liste) → Excitement level na Gameu

Postoji li seed za unos nekih inicijalnih vrijednosti (primjerice, gradovi i slično) → Role

Jesu li ispravno implementirane migracijske skripte (postoji li initial i bar jos jedna migracija) → Initial + RemoveOIBAndAddWillInvitePlayers

Postoje li barem 3 elementa na sučelju implementirani pomoću Tag Helper-a? → Većina linkova

Postoji li "delete" implementiran pomoću AJAX poziva? → delete Gamea

Postoji li DAL i model sloj? → postoje

Jesu li ispravni elementi u svakom sloju? → jesu

Postoje li odvojene role za neke dijelove aplikacije? → Admin može sve, Manager samo gledati i gledati detalje, Logiran User gledati bez detalja

Da li je Owin model ukombiniran sa vlastitom bazom? → Dodan WillInvitePlayer na AppUser-a?

Da li je moguće registrirati korisnika (obično + jedan od servisa kao što je google ili FB)? → Da, Google OAuth

Postoji li mogućnost dohvata barem jednog tipa entiteta putem API-ja? (lista, preko id-a) → Game

Postoji li mogućnost dodavanja, izmjene i brisanja barem jednog entiteta putem API-ja? → Game

Opcija 2: Koristiti vlastitu virtualku / alternativni provider → Hostano na mom Ubuntu Server

Ranija Predaja 6.6 → da











