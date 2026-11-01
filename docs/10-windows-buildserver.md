# Windows-Zweitmaschine (3090) — Git, SSH, Remote-Builds, Fernsteuerung

**Rolle der Maschine:** Build-Runner + Windows-Teststation + deine Workstation (z. B. für Videobearbeitung am Ende).
**Zeitbedarf:** 45–60 Min in 4 Häppchen. **Einordnung: Parallel-Track!** Dieses Setup darf M0/M1 am 02.11. nicht verdrängen — ideal abends oder während der Unity-Downloads auf dem Mac laufen.

## 0. Rolle: Unity-Build-Runner (D4 bleibt)

Die Kiste läuft **Unity 6 — dieselbe Version wie der Mac**, kein Unreal. Gründe: Der C#-Kern + Selbsttest ist Unity/.NET, das Meta XR All-in-One SDK ist der Unity-Pfad, und 17 Tage Restzeit vertragen keinen Engine-Wechsel.

## 1. Die Antwort vorweg: Was braucht wer?

| Wer | Braucht | Braucht nicht |
|---|---|---|
| **Ich (AutoClaw, auf dem Mac)** | **Git** (Code-Transport) + **SSH** (Befehle auf der Kiste) + **scp** (APK zurück) | keine Fernsteuerung, keine Freigabe |
| **Du, von unterwegs** | Fernsteuerung: **Parsec** (empfohlen) oder RDP (nur Windows Pro) — für Unity-Editor/Blick aufs Geschehen | — |
| Große Dateien (Videos) | optional SMB-Freigabe ODER einfach scp | — |

**Wichtiges Mental-Modell:** AutoClaw zieht NICHT auf die Windows-Kiste um. Ich bleibe auf dem Mac; das **Repo ist der Transportweg**, SSH ist mein Arm auf der anderen Maschine. (Später möglich: ein eigener AutoClaw dort mit eigenem Handoff-Prompt — für unseren Rhythmus aber ein Agent mehr als nötig.)

## 2. A — Git + GitHub (~15 Min)

1. github.com → **privates Repo** `hausgedaechtnis` anlegen (leer, ohne README)
2. **Mac-Seite übernehme ich** (siehe §6) — sobald du mir die Repo-URL gibst
3. Windows: git-scm.com → **Git für Windows** installieren (Standard-Optionen)
4. Auth für den Clone (eines von beiden):
   - **PAT (einfach):** GitHub → Settings → Developer settings → Personal access tokens → Generate, Scope `repo` (read reicht)
   - **Deploy Key (sauberer, nur lesend):** Repo → Settings → Deploy keys → Public-Key hinterlegen
5. Clone:
   ```powershell
   git clone https://github.com/<du>/hausgedaechtnis.git C:\Dev\hausgedaechtnis
   ```

## 3. B — OpenSSH-Server auf Windows (~10 Min)

PowerShell **als Administrator**:

```powershell
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0
Start-Service sshd
Set-Service -Name sshd -StartupType 'Automatic'
Get-NetFirewallRule -Name *ssh*   # sollte eine Allow-Regel fuer Port 22 zeigen
```

IP notieren: `ipconfig` (IPv4-Adresse). Erster Test vom Mac (Windows-Passwort deines Nutzers):

```bash
ssh <windows-user>@<ip> "echo ok"
```

**Key-Auth statt Passwort** (einmalig, übernehme ich auf der Mac-Seite; du fügst den Key ein):

```bash
# Mac:
ssh-keygen -t ed25519        # falls noch kein Key existiert
cat ~/.ssh/id_ed25519.pub    # Inhalt kopieren
```

```powershell
# Windows — DIE ADMIN-FALLE: Fuer Nutzer in der Gruppe "Administratoren" liest sshd
# NICHT ~/.ssh/authorized_keys, sondern:
notepad $env:ProgramData\ssh\administrators_authorized_keys   # Public-Key hier rein
icacls "$env:ProgramData\ssh\administrators_authorized_keys" /inheritance:r /grant "SYSTEM:F" /grant "BUILTIN\Administrators:F"
```

(Ohne die icacls-Zeile lehnt sshd den Key stillschweigend ab — klassischer Frustfehler.)

## 4. C — Unity auf der Windows-Kiste (~15 Min, viel Download)

1. Unity Hub → **Unity 6 LTS in exakt derselben Version wie auf dem Mac** + Module: **Android Build Support, OpenJDK, Android SDK & NDK**
2. Unity Hub **einmal manuell anmelden** (Personal License) — der Batch-Modus braucht die aktivierte Lizenz
3. Projekt öffnen: `C:\Dev\hausgedaechtnis\unity`
4. Package Manager: **Meta XR All-in-One** + **Meta XR Simulator** (identisch zum Setup-Guide §3)
5. Menü **Hausgedächtnis → Kern-Selbsttest** → muss „bestanden" melden

## 5. D — Remote-Build (~5 Min, das eigentliche Ziel)

Im Repo-Root liegt `build.cmd` (geliefert). Es findet Unity selbst, macht `git pull` und baut die APK im Batch-Modus. Von der Mac-Seite ist der komplette Build dann EIN Befehl, den ich fahre:

```bash
ssh <windows-user>@<ip> "C:\Dev\hausgedaechtnis\build.cmd"
```

APK zurück auf den Mac:

```bash
scp <windows-user>@<ip>:C:/Dev/hausgedaechtnis/unity/Builds/hausgedaechtnis.apk ~/Downloads/
```

(Beachte die Schrägstriche im Windows-Pfad bei scp — `C:/...` statt `C:\...`.)

Stolperfallen:
- **Editor zu?** Wenn das Projekt auf der Windows-Kiste im Editor offen ist, verweigert der Batch-Build (Projekt-Lock). Editor schließen vorher.
- `build.cmd` nutzt die neueste Hub-Installation; falls mehrere Versionen: `set FORCE_UNITY=C:\Pfad\zu\Unity.exe` vor dem Aufruf.

## 6. Mac-Seite — übernehme ich (~5 Min)

```bash
cd <workspace>/hausgedaechtnis
git init
git add -A && git commit -m "Kern: SDK-freier C#-Kern, Docs, Pläne, Build-Runner"
git remote add origin <repo-url>
git push -u origin main
```

**Was ich dafür von dir brauche:** privates GitHub-Repo anlegen + Repo-URL sagen. Fertig.

## 7. E — Fernsteuerung (für DICH, ~10 Min, optional)

- **Parsec (empfohlen):** läuft auch auf Windows Home, flüssig bei GPU-Inhalt, gratis für privat. Auf beiden Seiten installieren, fertig.
- **RDP:** nur **Windows Pro** (prÜfen: `msinfo32`). Freigabe: `SystemPropertiesRemote` → „Remotedesktop zulassen". Mac-Client: „Windows App" (App Store, ehemals MS Remote Desktop).
- **Übers Internet:** NIEMALS Ports (22/3389) am Router forwarden. Stattdessen **Tailscale** auf beiden Maschinen (privates Overlay-Netz, gratis) — danach ssh/Parsec/RDP überall wie im heimischen LAN.

## 8. F — Freigaben (SMB): nur falls wirklich gebraucht

Für Code: **nie** — dafür ist Git da. Einziger sinnvoller Fall: großer Binäraustausch (Quest-Video-Rohmaterial) — Ordner freigeben (Rechtsklick → Eigenschaften → Freigabe) und vom Mac per `smb://<ip>` mounten. scp tut es aber meist auch.

## 9. Was ich kann / nicht kann (mit Git+SSH)

| Aufgabe | Weg | Status |
|---|---|---|
| Code schreiben, committen, pushen | Mac, direkt | ✅ |
| Windows pullen lassen | `build.cmd` macht `git pull` mit | ✅ |
| APK-Build anstoßen | `ssh … build.cmd` → Unity batch mode | ✅ |
| Build-Log lesen | `ssh … type C:\Dev\hausgedaechtnis\unity\build.log` | ✅ |
| APK zurückholen | `scp` | ✅ |
| Unity-Editor interaktiv steuern | via SSH nicht möglich | ❌ → du (Parsec/RDP) oder ich auf dem **Mac** via MCP |

MCP über LAN (Unity AI Assistant auf der Windows-Kiste, Port nach außen): können wir gern testen — aber nicht einplanen. Der Plan of record ist **Git + SSH**.

## 10. Security-Kompakt

- Repo **privat**; Keystores/Secrets niemals ins Repo (.gitignore blockt sie)
- SSH mit **Key-Auth**; sshd nur im LAN oder hinter Tailscale
- **Kein Port-Forwarding** von 22/3389 ins Internet
- Windows-Nutzer mit Passwort (ssh benötigt das), Auto-Login aus lassen

## 11. Reihenfolge-Empfehlung

| Wenn | Dann |
|---|---|
| 02.11. vormittags | Erst Mac-Setup + M0/M1 (kritischer Pfad!) |
| 02.11. abends / Downloads auf dem Mac | §A–§D auf der Windows-Kiste |
| 03.11.+ | Ersten echten Remote-Build als Probe laufen lassen (sobald Szene existiert) |
| irgendwann | §7 Fernsteuerung + ggf. Tailscale, wenn du von außerhalb willst |
