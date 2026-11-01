# ADR 0004 — Eigener PDF-Emitter ohne Dependency

**Status:** akzeptiert (01.11.2026, Risiko R4)

## Kontext
Der PDF-Grundriss ist das Artefakt, das Bauherr und Folgefirma tatsächlich annehmen. Das Unity-Ökosystem bietet dafür nur schwere oder paid Plugins; externe Libraries sind IL2CPP- und Lizenz-Risiken.

## Entscheidung
`Runtime/Export/PdfEmitter.cs` schreibt PDF 1.4 selbst: A4, Base14-Helvetica, Linien/Text/Farben, exakt mitgezählte xref-Offsets. Umlaute werden transliteriert (ae/oe/ue/ss), um Encoding-Defekte grundsätzlich auszuschließen. Fallback: PNG-Render derselben Ansicht.

## Alternativen
- **iTextSharp / PDFsharp:** verworfen — Lizenz- und Gewichtsthemen, Unity-Kompatibilität unklar.
- **PNG statt PDF:** als Fallback behalten; PDF ist das Format, das angenommen wird.

## Konsequenzen
- Keine externen Dependencies im Projekt — auch keine Lizenzthemen.
- Umfang bewusst begrenzt (Linien + Text); Kurven/Grafiken nur bei Bedarf.
- Der Selbsttest deckt die Struktur ab (Header, xref-Offset, Transliteration).
