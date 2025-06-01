# 🎲 PhotoRoulette - Backend

Ovaj projekt predstavlja pozadinski (backend) sustav za Android aplikaciju "PhotoRoulette", razvijen u .NET tehnologiji. Aplikacija omogućuje više igrača da sudjeluju u igri gdje pokušavaju pogoditi tko je poslao koju fotografiju. Backend je implementiran tako da koristi SignalR za komunikaciju u stvarnom vremenu i sinkronizaciju stanja igre među klijentima. Server omogućuje pokretanje više paralelnih igara (lobbyja), upravljanje rundama, slanje i pohranu fotografija te automatsko bodovanje rezultata. Igrači se mogu pridružiti igri unosom naziva željenog lobbyja. Sustav podržava i naknadne izmjene rezultata i administraciju već završenih igara.


## 🔧 Funkcionalnosti

- ✅ Upravljanje višestrukim igrama u stvarnom vremenu putem **SignalR WebSocketa**.
- 🎮 Mogućnost **pokretanja igre**, pridruživanja igrača, automatsko odigravanje rundi te **automatsko izračunavanje rezultata**.
- 🖼️ Pregled svih **odigranih igara** i njihovih rezultata nakon završetka.
- 📝 **Naknadna izmjena podataka** nakon završetka igre.
- ✉️ **Pozivanje igrača u određeni lobby** putem pozivnica ili poveznica.
- 🔄 Dostupan **API za dohvat podataka** o igrama, korisnicima i rezultatima.