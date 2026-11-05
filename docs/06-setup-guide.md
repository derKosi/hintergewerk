# Setup-Guide (Zeitbox: 90 Minuten) — läuft auf dem Mac oder der Windows-Kiste

Ziel: Unity-Projekt steht, Meta XR SDK ist drin, MCP-Server verbunden, unser Kern-Ordner kompiliert.

## 0. Arbeitskopie auf dem Mac

Die Mac-Arbeitskopie liegt unter **`~/Documents/Dev/hintergewerk`** (Git-Klone, komplette Historie).
Der Agent arbeitet im geschützten AutoClaw-Arbeitsbereich und hält die Kopie per `git push dev` aktuell;
Unity Hub öffnet später **`~/Documents/Dev/hintergewerk/unity`**. GitHub kommt als dritte Station dazu,
sobald das Repo existiert (docs/10 §A). Workflow: Agent commitet → `git push dev` → du ziehst mit `git pull`
→ du commitest → Agent zieht. Regel: Nie parallel an derselben Datei arbeiten.

**Regel: Bei jedem Schritt, der länger als 20 Minuten hakt → notieren, überspringen, weiter.**
Menünamen können je nach Version leicht abweichen — dann in Package Manager / Project Settings nach dem Stichwort suchen.

## 1. Unity installieren (falls nicht vorhanden) — ~30 min

- [ ] Unity Hub → Installs → Install Editor → **Unity 6 LTS** (letzte LTS-Patchversion)
- [ ] Module mitinstallieren: **Android Build Support** + **OpenJDK** + **Android SDK & NDK Tools**
- [ ] Unity Hub → Sign-in mit Unity-ID (kostenlos), ggf. Personal License aktivieren

## 2. Projekt anlegen — ~5 min

- [ ] New Project → **3D (URP)** — Render-Pipeline URP ist der Meta-Empfehlungspfad
- [ ] Name: `Hintergewerk`, Ort nach Geschmack
- [ ] Edit → Project Settings → Editor → „Enter Play Mode Options" NICHT aktivieren (Meta SDK verträgt sich schlecht mit Fast-Enter)

## 3. Meta XR SDK installieren — ~15 min

> **06.11. aktualisiert (D26/D27):** Schritte sind auf der Windows-Kiste bereits erledigt
> (Editor-Werkzeug `Hintergewerk.EditorTools.ProjectSetup.Apply()` reproduziert alles).

- [x] SDK via **Metas NPM-Registry**: `Packages/manifest.json` → scopedRegistries
      (`https://npm.developer.oculus.com/`, scopes `["com.meta"]`) + `com.meta.xr.sdk.core: 207.0.1`
      (das Asset-Store-All-in-One-Paket wird nicht benötigt)
- [x] Project Settings → XR Plug-in Management → Android-Tab → **OpenXR** aktiv
- [x] OpenXR (Android) → **Meta Quest Support** aktiv + Meta XR Feature Set enabled;
      Hand Tracking läuft über OVR (OculusProjectConfig: `handTrackingSupport=ControllersAndHands`,
      `handTrackingFrequency=High`) — kein separater OpenXR-Interaction-Profile-Klick nötig
- [x] Player (Android): **IL2CPP**, **ARM64**, **minSdk 32** (Meta-Vorgabe), Bundle-ID **de.bautoolz.hintergewerk**
- [ ] Optional (Editor-Tests ohne Gerät): Meta XR Simulator — MSI liegt in `_hgw_unity_tmp/sim/`,
      Doppelklick, dann `Window/Meta/Meta XR Simulator/Activate` (D29)

## 4. MCP-Server (damit ich arbeiten kann) — ~10 min

- [ ] Window → Package Manager → Unity Registry → **AI Assistant** (Unity 6) → Install
- [ ] Nach der Installation: zugehöriges Fenster öffnen (Window → AI …), Unity-Cloud-Konto verbinden
- [ ] In den Einstellungen des AI Assistant den **MCP-Server aktivieren** (Standard-Port meist 6400)
- [ ] Prüfen: Editor läuft, MCP-Server logged „listening" — **dann sag mir Bescheid, ich übernehme**

## 5. Quest 3 vorbereiten — ~5 min (parallel machbar)

- [ ] Handy: Meta Horizon App → Headset koppeln → Einstellungen → Developer Mode aktivieren
  (Voraussetzung: Developer-Organisation unter developers.meta.com angelegt — ist mit dem Start-Programm-Konto da)
- [ ] Headset: USB-Kabel zum Mac, RSA-Fingerabdruck-Dialog im Headset bestätigen

## 6. Kern-Code einziehen + verifizieren — ~10 min

- [ ] `hintergewerk/unity/Assets/Hintergewerk/` → in `<Projekt>/Assets/` kopieren
- [ ] Warten bis Kompilierung durchläuft (Console: keine roten Fehler — sonst mir sagen)
- [ ] Menü **Hintergewerk → Kern-Selbsttest (JSON-Roundtrip)**: muss „bestanden" melden
- [ ] Menü **Hintergewerk → PDF-Beispiel erzeugen**: PDF öffnen und ansehen (Legende, Maßstab, Korridore)

## 7. Repo — ~5 min

- [ ] `git init`, Unity-`.gitignore` (Library/, Temp/, Logs/, UserSettings/, *.csproj, *.sln)
- [ ] Push zu GitHub **privat** (D7 — falls Forgejo gewünscht, igual; Entscheidung morgen)
- [ ] `docs/` aus diesem Workspace mit committen

## 8. Danach: Smoketest (Protokoll 07)

Wenn §1–§6 grün sind: direkt in `docs/07-smoketest-protokoll.md` — Build → Channel → Invite-URL → Gerät → Anker-Test.
Ich baue die Minimal-Szene dafür via MCP, sobald der Server läuft.

---

**Abbruchkriterien:** Unity-Download zu langsam → weiter mit Quest-Setup + Dashboard (§5 + Protokoll §A).
MCP will nicht → ich liefere Skripte/Szenen-Beschreibungen, du ziehst sie manuell ein (Fallback R6).
