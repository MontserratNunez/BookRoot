using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Application.ExportLayouts
{
    public static class ManuscriptLayout
    {
        public static string Generate(
            string username,
            IEnumerable<IGrouping<int, Domain.Entities.Interaction>> grouped,
            Dictionary<string, Domain.Entities.BookMetadata> bookDict,
            string filterLabel,
            int totalBooks)
        {
            var monthNames = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
            var booksHtml = new StringBuilder();

            foreach (var yearGroup in grouped)
            {
                booksHtml.Append($@"
                <section class='year-block'>
                    <div class='year-heading'>
                        <span class='year-rule'></span>
                        <h2 class='year-title'>{ToRoman(yearGroup.Key)}<span class='year-arabic'>{yearGroup.Key}</span></h2>
                        <span class='year-rule'></span>
                    </div>");

                var monthGroups = yearGroup
                    .GroupBy(x => x.FinishedAt!.Value.Month)
                    .OrderByDescending(g => g.Key);

                foreach (var monthGroup in monthGroups)
                {
                    var monthName = monthNames[monthGroup.Key - 1];
                    var monthBooks = monthGroup.Where(x => bookDict.ContainsKey(x.BookId)).OrderByDescending(x => x.FinishedAt).ToList();
                    if (monthBooks.Count == 0) continue;

                    booksHtml.Append($@"
                    <div class='month-block'>
                        <h3 class='month-title'><span class='month-glyph'>{ColumnGlyph}</span>{WebUtility.HtmlEncode(monthName)}<span class='month-count'>{monthBooks.Count} {(monthBooks.Count == 1 ? "obra" : "obras")}</span></h3>
                        <div class='books-grid'>");

                    foreach (var interaction in monthBooks)
                    {
                        var book = bookDict[interaction.BookId];
                        var day = interaction.FinishedAt?.ToString("dd");
                        var ratingHtml = BuildRatingLaurels(interaction.Rating);

                        booksHtml.Append($@"
                            <article class='book-card'>
                                <div class='book-day'>{day}</div>
                                <div class='book-divider'></div>
                                <div class='book-info'>
                                    <h4 class='book-title'>{WebUtility.HtmlEncode(book.Title)}</h4>
                                    <div class='book-author'>{WebUtility.HtmlEncode(book.Author)}</div>
                                    <div class='book-rating'>{ratingHtml}</div>
                                </div>
                            </article>");
                    }

                    booksHtml.Append("</div></div>");
                }

                booksHtml.Append("</section>");
            }

            if (totalBooks == 0)
            {
                booksHtml.Append($@"
                <div class='empty-state'>
                    {EmptyScrollIcon}
                    <p>No se han inscrito lecturas en este período.</p>
                </div>");
            }

            var css = BuildCss();

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Códice de Lecturas — {WebUtility.HtmlEncode(username)}</title>
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link href=""https://fonts.googleapis.com/css2?family=Cinzel:wght@400;500;600;700&family=EB+Garamond:ital,wght@0,400;0,500;0,600;1,400&display=swap"" rel=""stylesheet"">
    <style>{css}</style>
</head>
<body>
    <div class='manuscript'>
        <div class='pediment'>
            <div class='column column-left'>{ColumnIcon}</div>
            <div class='pediment-center'>
                <div class='laurel'>{LaurelIcon}</div>
                <div class='header-badge'>Códice de Lecturas</div>
            </div>
            <div class='column column-right'>{ColumnIcon}</div>
        </div>

        <header class='header'>
            <h1>Compendio Librario de <span>{WebUtility.HtmlEncode(username)}</span></h1>
            <div class='meta'>{WebUtility.HtmlEncode(filterLabel)} &middot; Inscrito el {DateTime.Now:dd 'de' MMMM 'de' yyyy}</div>
        </header>

        <div class='stats-bar'>
            <div class='wax-seal'>
                {WaxSealIcon}
                <div class='seal-content'>
                    <div class='stat-num'>{totalBooks}</div>
                    <div class='stat-label'>{(totalBooks == 1 ? "Obra Sellada" : "Obras Selladas")}</div>
                </div>
            </div>
        </div>

        <div class='timeline'>
            {booksHtml}
        </div>

        <footer class='footer'>
            <span class='footer-rule'></span>
            <span>Creado con BookRoot &middot; bookroot.net</span>
            <span class='footer-rule'></span>
        </footer>
    </div>
</body>
</html>";
        }

        private static string BuildRatingLaurels(int? rating)
        {
            if (!rating.HasValue)
            {
                return "<span class='no-rating'>Sin calificación</span>";
            }

            var sb = new StringBuilder();
            for (int i = 1; i <= 5; i++)
            {
                var filled = i <= rating.Value;
                sb.Append($"<span class='leaf {(filled ? "filled" : "")}'>{LeafIcon}</span>");
            }
            return sb.ToString();
        }

        
        private static string ToRoman(int number)
        {
            if (number <= 0 || number > 3999) return "";
            var values = new (int val, string sym)[]
            {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
                (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };
            var sb = new StringBuilder();
            var n = number;
            foreach (var (val, sym) in values)
            {
                while (n >= val)
                {
                    sb.Append(sym);
                    n -= val;
                }
            }
            return sb.ToString();
        }

        private const string LeafIcon = @"<svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'>
            <path d='M12 2 C18 4 20 10 16 16 C13 20 8 21 4 20 C6 12 7 5 12 2 Z'/>
            <path d='M6 19 C10 14 12 9 13 4' fill='none' class='vein'/>
        </svg>";

        private const string ColumnGlyph = @"<svg viewBox='0 0 20 28' xmlns='http://www.w3.org/2000/svg' class='column-glyph'>
            <rect x='2' y='2' width='16' height='2.4' />
            <rect x='4' y='5' width='2' height='17' /><rect x='9' y='5' width='2' height='17' /><rect x='14' y='5' width='2' height='17' />
            <rect x='2' y='23.6' width='16' height='2.4' />
        </svg>";

        private static readonly string ColumnIcon = @"<svg viewBox='0 0 60 220' xmlns='http://www.w3.org/2000/svg'>
            <rect x='4' y='2' width='52' height='12' class='stone' />
            <rect x='0' y='14' width='60' height='8' class='stone-light' />
            <rect x='9' y='24' width='4' height='176' class='flute' /><rect x='18' y='24' width='4' height='176' class='flute' />
            <rect x='27' y='24' width='6' height='176' class='flute' /><rect x='38' y='24' width='4' height='176' class='flute' />
            <rect x='47' y='24' width='4' height='176' class='flute' />
            <rect x='0' y='200' width='60' height='8' class='stone-light' />
            <rect x='4' y='208' width='52' height='12' class='stone' />
        </svg>";

        private const string LaurelIcon = @"<svg viewBox='0 0 120 60' xmlns='http://www.w3.org/2000/svg'>
            <g class='sprig-left'>
                <path d='M58 50 C40 46 24 36 16 20' fill='none' class='stem'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(4,2)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(12,10)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(20,18)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(28,26)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(36,33)'/>
            </g>
            <g class='sprig-right' transform='translate(120,0) scale(-1,1)'>
                <path d='M58 50 C40 46 24 36 16 20' fill='none' class='stem'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(4,2)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(12,10)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(20,18)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(28,26)'/>
                <path d='M20 24 C16 20 16 15 20 11 C24 15 24 20 20 24 Z' transform='translate(36,33)'/>
            </g>
        </svg>";

        private const string WaxSealIcon = @"<svg viewBox='0 0 120 120' xmlns='http://www.w3.org/2000/svg' class='seal-svg'>
            <circle cx='60' cy='60' r='56' class='seal-ridge'/>
            <circle cx='60' cy='60' r='48' class='seal-face'/>
            <circle cx='60' cy='60' r='48' class='seal-inner-ring' fill='none'/>
        </svg>";

        private const string EmptyScrollIcon = @"<svg viewBox='0 0 64 64' xmlns='http://www.w3.org/2000/svg' class='empty-scroll'>
            <rect x='10' y='16' width='44' height='32' class='parchment-body'/>
            <rect x='6' y='14' width='10' height='36' rx='5' class='scroll-rod'/>
            <rect x='48' y='14' width='10' height='36' rx='5' class='scroll-rod'/>
            <path d='M18 26 H46 M18 34 H46 M18 42 H36' class='lines' stroke-width='2'/>
        </svg>";

        private static string BuildCss()
        {
            return @"
                :root {
                    --void-bg: #C9BFA0;
                    --parchment: #F1E6C6;
                    --parchment-dark: #E4D5A9;
                    --ink: #3E2E1E;
                    --ink-soft: #6B5A42;
                    --oxblood: #7A2E2E;
                    --gold: #A9803D;
                    --stone: #B7AC8C;
                    --stone-light: #D3C7A0;
                    --line: #D3C3A0;
                }

                * { box-sizing: border-box; margin: 0; padding: 0; }

                body {
                    background: radial-gradient(circle at 50% 0%, #DCCFA5 0%, var(--void-bg) 60%, #BBAF8C 100%);
                    font-family: 'EB Garamond', serif;
                    color: var(--ink);
                    padding: 40px 20px 90px;
                    display: flex;
                    justify-content: center;
                }

                .manuscript {
                    max-width: 860px;
                    width: 100%;
                    background: var(--parchment);
                    background-image:
                        radial-gradient(circle at 8% 12%, rgba(122,46,46,0.05), transparent 30%),
                        radial-gradient(circle at 92% 85%, rgba(169,128,61,0.08), transparent 35%);
                    box-shadow: 0 40px 90px -30px rgba(35, 26, 15, 0.45);
                    padding: 0 66px 56px;
                    clip-path: polygon(
                        0% 1%, 8% 0%, 16% 1.3%, 24% 0.2%, 32% 1.1%, 40% 0%, 48% 1.2%, 56% 0.3%, 64% 1%, 72% 0%, 80% 1.3%, 88% 0.2%, 100% 1%,
                        100% 99%, 92% 100%, 84% 98.7%, 76% 99.8%, 68% 98.9%, 60% 100%, 52% 98.8%, 44% 99.7%, 36% 100%, 28% 98.9%, 20% 99.8%, 12% 100%, 0% 99%
                    );
                    position: relative;
                }

                .pediment { display: flex; align-items: stretch; justify-content: center; gap: 18px; padding-top: 26px; }
                .column { width: 34px; flex-shrink: 0; opacity: 0.9; }
                .column svg { width: 100%; height: 130px; }
                .column .stone { fill: var(--stone); } .column .stone-light { fill: var(--stone-light); } .column .flute { fill: var(--parchment-dark); }
                .pediment-center { text-align: center; padding-top: 6px; }
                .laurel svg { width: 100px; height: 50px; }
                .laurel .stem { stroke: var(--gold); stroke-width: 1.6; }
                .laurel path:not(.stem) { fill: var(--oxblood); opacity: 0.85; }
                .header-badge {
                    font-family: 'Cinzel', serif;
                    letter-spacing: 0.3em;
                    text-transform: uppercase;
                    font-size: 11px;
                    color: var(--gold);
                    margin-top: 2px;
                }

                .header { text-align: center; padding: 18px 0 30px; border-bottom: 2px solid var(--line); }
                h1 {
                    font-family: 'Cinzel', serif;
                    font-weight: 600;
                    font-size: 36px;
                    color: var(--ink);
                    letter-spacing: 0.01em;
                }
                h1 span { color: var(--oxblood); }
                .meta {
                    font-family: 'EB Garamond', serif;
                    font-style: italic;
                    font-size: 14px;
                    color: var(--ink-soft);
                    margin-top: 12px;
                    text-transform: capitalize;
                }

                .stats-bar { display: flex; justify-content: center; padding: 38px 0 14px; }
                .wax-seal { position: relative; width: 130px; height: 130px; display: flex; align-items: center; justify-content: center; }
                .seal-svg { position: absolute; inset: 0; width: 100%; height: 100%; }
                .seal-ridge { fill: var(--oxblood); }
                .seal-face { fill: #8C3A3A; }
                .seal-inner-ring { stroke: rgba(241,230,198,0.4); stroke-width: 1.5; stroke-dasharray: 3 4; }
                .seal-content { position: relative; text-align: center; }
                .stat-num { font-family: 'Cinzel', serif; font-size: 34px; font-weight: 700; color: var(--parchment); line-height: 1; }
                .stat-label {
                    font-family: 'Cinzel', serif;
                    font-size: 8.5px;
                    letter-spacing: 0.08em;
                    text-transform: uppercase;
                    color: var(--parchment);
                    opacity: 0.85;
                    margin-top: 6px;
                }

                .timeline { margin-top: 20px; }

                .year-block { margin-top: 42px; }
                .year-block:first-child { margin-top: 26px; }
                .year-heading { display: flex; align-items: center; gap: 16px; margin-bottom: 20px; }
                .year-rule { flex: 1; height: 1px; background: var(--line); position: relative; }
                .year-rule::after { content: ''; position: absolute; right: 0; top: -2px; width: 5px; height: 5px; background: var(--gold); transform: rotate(45deg); }
                .year-title { font-family: 'Cinzel', serif; font-size: 24px; color: var(--oxblood); font-weight: 600; white-space: nowrap; letter-spacing: 0.06em; }
                .year-arabic { font-family: 'EB Garamond', serif; font-size: 14px; color: var(--ink-soft); font-style: italic; margin-left: 8px; letter-spacing: 0; }

                .month-block { margin-top: 26px; }
                .month-title {
                    font-family: 'Cinzel', serif;
                    font-weight: 500;
                    font-size: 13px;
                    letter-spacing: 0.2em;
                    text-transform: uppercase;
                    color: var(--ink);
                    display: flex;
                    align-items: center;
                    gap: 9px;
                    padding-bottom: 10px;
                    border-bottom: 1px solid var(--line);
                    margin-bottom: 16px;
                }
                .month-glyph svg { width: 14px; height: 20px; fill: var(--gold); }
                .month-count { font-family: 'EB Garamond', serif; font-style: italic; letter-spacing: 0; text-transform: none; font-size: 13px; color: var(--ink-soft); margin-left: auto; }

                .books-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 14px; }

                .book-card {
                    display: flex; gap: 14px; align-items: flex-start;
                    background: rgba(255,252,240,0.4);
                    border: 1px solid var(--line);
                    padding: 16px 18px;
                    position: relative;
                }
                .book-card::before, .book-card::after {
                    content: ''; position: absolute; width: 7px; height: 7px; border: 1px solid var(--gold); opacity: 0.6;
                }
                .book-card::before { top: 4px; left: 4px; border-right: none; border-bottom: none; }
                .book-card::after { bottom: 4px; right: 4px; border-left: none; border-top: none; }

                .book-day { font-family: 'Cinzel', serif; font-size: 22px; color: var(--oxblood); font-weight: 600; min-width: 30px; }
                .book-divider { width: 1px; align-self: stretch; background: var(--line); }
                .book-info { flex: 1; min-width: 0; }
                .book-title { font-family: 'Cinzel', serif; font-size: 15px; font-weight: 600; color: var(--ink); line-height: 1.4; letter-spacing: 0.01em; }
                .book-author { font-family: 'EB Garamond', serif; font-style: italic; font-size: 13px; color: var(--ink-soft); margin-top: 4px; }
                .book-rating { margin-top: 10px; display: flex; gap: 3px; align-items: center; }
                .leaf svg { width: 14px; height: 14px; fill: var(--line); }
                .leaf.filled svg { fill: var(--oxblood); }
                .leaf .vein { stroke: var(--parchment); }
                .leaf.filled .vein { stroke: var(--parchment); }
                .no-rating { font-family: 'EB Garamond', serif; font-style: italic; font-size: 12px; color: var(--ink-soft); }

                .empty-state { text-align: center; padding: 60px 20px; color: var(--ink-soft); }
                .empty-scroll { width: 60px; height: 60px; margin-bottom: 14px; }
                .empty-scroll .parchment-body { fill: var(--parchment-dark); stroke: var(--gold); stroke-width: 1; }
                .empty-scroll .scroll-rod { fill: var(--gold); opacity: 0.7; }
                .empty-scroll .lines { stroke: var(--ink-soft); opacity: 0.5; }
                .empty-state p { font-family: 'EB Garamond', serif; font-style: italic; font-size: 15px; }

                .footer {
                    text-align: center;
                    margin-top: 54px;
                    padding-top: 20px;
                    display: flex; align-items: center; gap: 16px;
                    font-family: 'Cinzel', serif;
                    font-size: 10px;
                    letter-spacing: 0.16em;
                    text-transform: uppercase;
                    color: var(--ink-soft);
                }
                .footer-rule { flex: 1; height: 1px; background: var(--line); }

                @media (max-width: 640px) {
                    body { padding: 20px 8px 60px; }
                    .manuscript { padding: 0 20px 36px; }
                    .column { display: none; }
                    h1 { font-size: 26px; }
                    .books-grid { grid-template-columns: 1fr; }
                }
            ";
        }
    }
}