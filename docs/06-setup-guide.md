# Setup-Guide — Di, 02.11. (Zeitbox: 90 Minuten)

Ziel: Unity-Projekt steht, Meta XR SDK ist drin, MCP-Server verbunden, unser Kern-Ordner kompiliert.
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

- [ ] Window → Package Manager → (Dropdown: Unity Registry) → **Meta XR All-in-One** → Install
  (Falls nicht gelistet: über „Add package by name": `com.meta.xr.sdk.all` — Wettbewerbsvorgabe v207+)
- [ ] Nach Installation öffnet sich ggf. der Meta-Setup-Wizard: **Build Target auf Android** umstellen lassen
- [ ] Window → Package Manager → **Meta XR Simulator** installieren (Editor-Tests ohne Gerät)
- [ ] Project Settings → XR Plug-in Management → Android-Tab → **OpenXR** anhaken
- [ ] OpenXR (Android) → Meta Quest Support anhaken; **Hand Tracking** → Interaction Profile „Meta Quest Hand Interaction" hinzufügen
- [ ] Project Settings → Player (Android): Scripting Backend **IL2CPP**, Target Architectures **ARM64** (arme64-v8a), Minimum API Level lt. SDK-Vorgabe (aktuell 29+)
- [ ] Player Settings → Bundle Identifier: **de.bautoolz.hintergewerk**

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
