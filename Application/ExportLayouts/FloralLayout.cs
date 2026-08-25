using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Application.ExportLayouts
{
    public static class FloralLayout
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
                        <span class='year-sprig'>{SprigIcon}</span>
                        <h2 class='year-title'>{yearGroup.Key}</h2>
                        <span class='year-line'></span>
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
                        <div class='month-stem'></div>
                        <h3 class='month-title'>{WebUtility.HtmlEncode(monthName)}<span class='month-count'>{monthBooks.Count} {(monthBooks.Count == 1 ? "lectura" : "lecturas")}</span></h3>
                        <div class='books-grid'>");

                    foreach (var interaction in monthBooks)
                    {
                        var book = bookDict[interaction.BookId];
                        var day = interaction.FinishedAt?.ToString("dd");
                        var ratingHtml = BuildRatingBlooms(interaction.Rating);

                        booksHtml.Append($@"
                            <article class='book-card'>
                                <div class='book-day'>
                                    <span class='day-num'>{day}</span>
                                    <span class='day-leaf'>{LeafIconSmall}</span>
                                </div>
                                <div class='book-info'>
                                    <h4 class='book-title'>{WebUtility.HtmlEncode(book.Title)}</h4>
                                    <div class='book-author'>{WebUtility.HtmlEncode(book.Author)}</div>
                                    <div class='book-rating' title='Calificación'>{ratingHtml}</div>
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
                    {EmptyPotIcon}
                    <p>Aún no hay lecturas registradas en este período.</p>
                </div>");
            }

            var css = BuildCss();

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Diario Botánico — {WebUtility.HtmlEncode(username)}</title>
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link href=""https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,500;0,600;0,700;1,500&family=EB+Garamond:ital,wght@0,400;0,500;1,400&family=Jost:wght@300;400;500;600&display=swap"" rel=""stylesheet"">
    <style>{css}</style>
</head>
<body>
    
    <div class='bg-flower sf-tl'>{SunflowerIcon}</div>
    <div class='bg-flower sf-tr'>{SunflowerIcon}</div>
    <div class='bg-flower sf-ml'>{SunflowerIcon}</div>
    <div class='bg-flower sf-mr'>{SunflowerIcon}</div>
    <div class='bg-flower sf-bl'>{SunflowerIcon}</div>
    <div class='bg-flower sf-br'>{SunflowerIcon}</div>

    <div class='corner corner-tl'>{BranchBig}</div>
    <div class='corner corner-tr'>{BranchBig}</div>
    <div class='corner corner-bl'>{BranchBig}</div>
    <div class='corner corner-br'>{BranchBig}</div>

    <div class='container'>
        <header class='header'>
            <div class='header-flourish'>{FlourishLeft}<span class='header-badge'>Diario Botánico</span>{FlourishRight}</div>
            <h1>Jardín de Lecturas de <span>{WebUtility.HtmlEncode(username)}</span></h1>
            <div class='meta'>{WebUtility.HtmlEncode(filterLabel)} &middot; Generado el {DateTime.Now:dd 'de' MMMM 'de' yyyy}</div>
        </header>

        <div class='stats-bar'>
            <div class='stat-item'>
                <span class='stat-flower'>{StatFlower}</span>
                <div class='stat-num'>{totalBooks}</div>
                <div class='stat-label'>{(totalBooks == 1 ? "Libro Florecido" : "Libros Florecidos")}</div>
            </div>
        </div>

        <div class='timeline'>
            {booksHtml}
        </div>

        <footer class='footer'>
            <span class='footer-sprig'>{SprigIcon}</span>
            Creado con BookRoot &middot; bookroot.net
        </footer>
    </div>
</body>
</html>";
        }

        private static string BuildRatingBlooms(int? rating)
        {
            if (!rating.HasValue)
            {
                return "<span class='no-rating'>Sin calificación</span>";
            }

            var sb = new StringBuilder();
            for (int i = 1; i <= 5; i++)
            {
                var filled = i <= rating.Value;
                sb.Append($"<span class='bloom {(filled ? "filled" : "")}'>{BloomIcon}</span>");
            }
            return sb.ToString();
        }

        private const string SunflowerIcon = @"<svg viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'>
            <g transform='translate(50,50)'>
                
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#E5A93C' transform='rotate(0)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#F4BC4B' transform='rotate(30)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#E5A93C' transform='rotate(60)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#F4BC4B' transform='rotate(90)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#E5A93C' transform='rotate(120)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#F4BC4B' transform='rotate(150)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#E5A93C' transform='rotate(180)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#F4BC4B' transform='rotate(210)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#E5A93C' transform='rotate(240)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#F4BC4B' transform='rotate(270)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#E5A93C' transform='rotate(300)'/>
                <path d='M0,-48 C6,-30 6,-18 0,-15 C-6,-18 -6,-30 0,-48 Z' fill='#F4BC4B' transform='rotate(330)'/>
                
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(15)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(45)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(75)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(105)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(135)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(165)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(195)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(225)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(255)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(285)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(315)'/>
                <path d='M0,-42 C4,-26 4,-16 0,-13 C-4,-16 -4,-26 0,-42 Z' fill='#D49228' transform='rotate(345)'/>
                
                <circle r='17' fill='#4A321A'/>
                <circle r='13' fill='#38220F'/>
                <circle r='8' fill='#261507' opacity='0.7'/>
            </g>
        </svg>";

        private const string BloomIcon = @"<svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'>
            <g transform='translate(12,12)'>
                <path d='M0,-8 C2.6,-5.4 2.6,-1.6 0,0 C-2.6,-1.6 -2.6,-5.4 0,-8 Z' transform='rotate(0)'/>
                <path d='M0,-8 C2.6,-5.4 2.6,-1.6 0,0 C-2.6,-1.6 -2.6,-5.4 0,-8 Z' transform='rotate(72)'/>
                <path d='M0,-8 C2.6,-5.4 2.6,-1.6 0,0 C-2.6,-1.6 -2.6,-5.4 0,-8 Z' transform='rotate(144)'/>
                <path d='M0,-8 C2.6,-5.4 2.6,-1.6 0,0 C-2.6,-1.6 -2.6,-5.4 0,-8 Z' transform='rotate(216)'/>
                <path d='M0,-8 C2.6,-5.4 2.6,-1.6 0,0 C-2.6,-1.6 -2.6,-5.4 0,-8 Z' transform='rotate(288)'/>
                <circle r='1.9' class='bloom-center'/>
            </g>
        </svg>";

        private const string LeafIconSmall = @"<svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'>
            <path d='M4 20 C4 10 12 4 20 4 C20 12 14 20 4 20 Z'/>
            <path d='M4 20 C9 15 13 11 20 4' fill='none' stroke-width='1' class='vein'/>
        </svg>";

        private const string SprigIcon = @"<svg viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg' class='sprig'>
            <path d='M16 30 C16 20 16 12 16 4' fill='none' class='stem'/>
            <path d='M16 22 C10 20 7 16 8 11 C13 12 16 16 16 22 Z'/>
            <path d='M16 16 C22 14 25 10 24 5 C19 6 16 10 16 16 Z'/>
            <circle cx='16' cy='4' r='2.4' class='bud'/>
        </svg>";

        private const string StatFlower = @"<svg viewBox='0 0 40 40' xmlns='http://www.w3.org/2000/svg'>
            <g transform='translate(20,20)'>
                <path d='M0,-14 C4.6,-9.6 4.6,-2.8 0,0 C-4.6,-2.8 -4.6,-9.6 0,-14 Z' transform='rotate(0)'/>
                <path d='M0,-14 C4.6,-9.6 4.6,-2.8 0,0 C-4.6,-2.8 -4.6,-9.6 0,-14 Z' transform='rotate(72)'/>
                <path d='M0,-14 C4.6,-9.6 4.6,-2.8 0,0 C-4.6,-2.8 -4.6,-9.6 0,-14 Z' transform='rotate(144)'/>
                <path d='M0,-14 C4.6,-9.6 4.6,-2.8 0,0 C-4.6,-2.8 -4.6,-9.6 0,-14 Z' transform='rotate(216)'/>
                <path d='M0,-14 C4.6,-9.6 4.6,-2.8 0,0 C-4.6,-2.8 -4.6,-9.6 0,-14 Z' transform='rotate(288)'/>
                <circle r='3.4' class='bloom-center'/>
            </g>
        </svg>";

        private const string EmptyPotIcon = @"<svg viewBox='0 0 64 64' xmlns='http://www.w3.org/2000/svg' class='empty-pot'>
            <path d='M20 30 L44 30 L40 54 L24 54 Z' class='pot'/>
            <rect x='16' y='24' width='32' height='7' rx='2' class='pot-rim'/>
            <path d='M32 24 C32 14 32 10 24 4' fill='none' class='stem'/>
            <path d='M32 20 C26 18 23 14 24 9 C29 10 32 14 32 20 Z'/>
            <path d='M32 14 C38 12 41 8 40 3 C35 4 32 8 32 14 Z'/>
        </svg>";

        private static readonly string FlourishLeft = @"<svg viewBox='0 0 60 20' xmlns='http://www.w3.org/2000/svg' class='flourish'>
            <path d='M58 10 C40 2 20 2 2 10' fill='none'/>
            <circle cx='2' cy='10' r='2' class='dot'/>
        </svg>";

        private static readonly string FlourishRight = @"<svg viewBox='0 0 60 20' xmlns='http://www.w3.org/2000/svg' class='flourish'>
            <path d='M2 10 C20 2 40 2 58 10' fill='none'/>
            <circle cx='58' cy='10' r='2' class='dot'/>
        </svg>";

        private const string BranchBig = @"<svg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'>
            <path d='M0 0 C40 30 60 70 50 120' fill='none' class='stem'/>
            <path d='M20 40 C40 42 55 55 54 75 C36 74 22 60 20 40 Z'/>
            <path d='M8 70 C28 68 45 78 47 98 C29 100 12 88 8 70 Z'/>
            <path d='M28 105 C46 100 62 108 68 126 C50 132 32 124 28 105 Z'/>
            <g transform='translate(50,120)'>
                <path d='M0,-11 C3.6,-7.6 3.6,-2.4 0,0 C-3.6,-2.4 -3.6,-7.6 0,-11 Z' transform='rotate(0)'/>
                <path d='M0,-11 C3.6,-7.6 3.6,-2.4 0,0 C-3.6,-2.4 -3.6,-7.6 0,-11 Z' transform='rotate(72)'/>
                <path d='M0,-11 C3.6,-7.6 3.6,-2.4 0,0 C-3.6,-2.4 -3.6,-7.6 0,-11 Z' transform='rotate(144)'/>
                <path d='M0,-11 C3.6,-7.6 3.6,-2.4 0,0 C-3.6,-2.4 -3.6,-7.6 0,-11 Z' transform='rotate(216)'/>
                <path d='M0,-11 C3.6,-7.6 3.6,-2.4 0,0 C-3.6,-2.4 -3.6,-7.6 0,-11 Z' transform='rotate(288)'/>
                <circle r='2.6' class='bloom-center'/>
            </g>
        </svg>";

        private static string BuildCss()
        {
            return @"
                :root {
                    --cream: #FBF6EC;
                    --paper: #FFFFFF;
                    --evergreen: #2F4A3D;
                    --evergreen-soft: #4A6B57;
                    --rose: #C08585;
                    --sage: #8FA283;
                    --gold: #B8935B;
                    --ink: #3A3229;
                    --ink-soft: #6B6154;
                    --line: #E3DAC8;
                }

                * { box-sizing: border-box; margin: 0; padding: 0; }

                body {
                    background: radial-gradient(circle at 15% 10%, #F8F2E4 0%, var(--cream) 45%, #F1E8D5 100%);
                    font-family: 'EB Garamond', serif;
                    color: var(--ink);
                    padding: 48px 24px 90px;
                    position: relative;
                    overflow-x: hidden;
                }

                
                .bg-flower {
                    position: fixed;
                    pointer-events: none;
                    z-index: 0;
                    opacity: 0.28;
                    filter: drop-shadow(0 8px 16px rgba(58, 50, 41, 0.08));
                }
                .bg-flower svg { width: 100%; height: 100%; }

                .sf-tl { top: -60px; left: -50px; width: 280px; height: 280px; transform: rotate(-15deg); }
                .sf-tr { top: -50px; right: -60px; width: 310px; height: 310px; transform: rotate(25deg); }
                .sf-ml { top: 30%; left: 20px; width: 320px; height: 320px; opacity: 0.22; transform: rotate(-40deg); }
                .sf-mr { top: 34%; right: 67px; width: 300px; height: 300px; opacity: 0.22; transform: rotate(18deg); }
                .sf-bl { bottom: -60px; left: -40px; width: 290px; height: 290px; transform: rotate(35deg); }
                .sf-br { bottom: -50px; right: -50px; width: 270px; height: 270px; transform: rotate(-22deg); }

                
                .corner { position: fixed; width: 190px; height: 190px; opacity: 0.45; pointer-events: none; z-index: 0; }
                .corner svg { width: 100%; height: 100%; fill: var(--sage); stroke: var(--sage); }
                .corner svg .stem { stroke: var(--evergreen-soft); stroke-width: 2; fill: none; }
                .corner svg .bloom-center { fill: var(--gold); stroke: none; }
                .corner-tl { top: -30px; left: -30px; }
                .corner-tr { top: -30px; right: -30px; transform: scaleX(-1); }
                .corner-bl { bottom: -30px; left: -30px; transform: scaleY(-1); }
                .corner-br { bottom: -30px; right: -30px; transform: rotate(180deg); }

                .container {
                    max-width: 880px;
                    margin: 0 auto;
                    background: var(--paper);
                    border: 1px solid var(--line);
                    border-radius: 4px;
                    box-shadow: 0 30px 70px -30px rgba(58, 50, 41, 0.25);
                    padding: 56px 64px 48px;
                    position: relative;
                    z-index: 1;
                }

                .header { text-align: center; padding-bottom: 32px; border-bottom: 1px solid var(--line); }
                .header-flourish { display: flex; align-items: center; justify-content: center; gap: 14px; margin-bottom: 18px; }
                .flourish { width: 60px; height: 20px; }
                .flourish path { stroke: var(--gold); stroke-width: 1.2; }
                .flourish .dot { fill: var(--gold); }
                .header-badge {
                    font-family: 'Jost', sans-serif;
                    letter-spacing: 0.28em;
                    text-transform: uppercase;
                    font-size: 11px;
                    color: var(--gold);
                    font-weight: 500;
                }
                h1 {
                    font-family: 'Playfair Display', serif;
                    font-weight: 600;
                    font-size: 40px;
                    color: var(--evergreen);
                    line-height: 1.25;
                }
                h1 span { font-style: italic; color: var(--rose); font-weight: 500; }
                .meta {
                    font-family: 'Jost', sans-serif;
                    font-size: 13px;
                    letter-spacing: 0.04em;
                    color: var(--ink-soft);
                    margin-top: 12px;
                    text-transform: capitalize;
                }

                .stats-bar { display: flex; justify-content: center; padding: 34px 0 8px; }
                .stat-item { text-align: center; }
                .stat-flower svg { width: 40px; height: 40px; fill: var(--rose); margin-bottom: 6px; }
                .stat-flower .bloom-center { fill: var(--gold); }
                .stat-num { font-family: 'Playfair Display', serif; font-size: 44px; font-weight: 700; color: var(--evergreen); line-height: 1; }
                .stat-label {
                    font-family: 'Jost', sans-serif;
                    font-size: 12px;
                    letter-spacing: 0.16em;
                    text-transform: uppercase;
                    color: var(--ink-soft);
                    margin-top: 6px;
                }

                .timeline { margin-top: 24px; }

                .year-block { margin-top: 44px; }
                .year-block:first-child { margin-top: 30px; }
                .year-heading { display: flex; align-items: center; gap: 12px; margin-bottom: 6px; }
                .year-sprig svg { width: 26px; height: 26px; }
                .sprig .stem { stroke: var(--evergreen-soft); stroke-width: 1.6; }
                .sprig path:not(.stem) { fill: var(--sage); }
                .sprig .bud { fill: var(--rose); }
                .year-title { font-family: 'Playfair Display', serif; font-size: 26px; color: var(--evergreen); font-weight: 600; white-space: nowrap; }
                .year-line { flex: 1; height: 1px; background: linear-gradient(90deg, var(--line), transparent); }

                .month-block { position: relative; padding-left: 28px; margin-top: 26px; }
                .month-stem { position: absolute; left: 6px; top: 6px; bottom: -6px; width: 2px; background: linear-gradient(var(--sage), var(--line)); border-radius: 2px; }
                .month-stem::before {
                    content: '';
                    position: absolute; left: -3px; top: -3px;
                    width: 8px; height: 8px; border-radius: 50%;
                    background: var(--rose); box-shadow: 0 0 0 4px var(--paper), 0 0 0 5px var(--line);
                }
                .month-title {
                    font-family: 'Jost', sans-serif;
                    font-weight: 600;
                    font-size: 13px;
                    letter-spacing: 0.22em;
                    text-transform: uppercase;
                    color: var(--evergreen-soft);
                    display: flex;
                    align-items: baseline;
                    gap: 10px;
                    margin-bottom: 16px;
                }
                .month-count { font-family: 'EB Garamond', serif; font-style: italic; letter-spacing: 0; text-transform: none; font-size: 14px; color: var(--ink-soft); }

                .books-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 14px; }

                .book-card {
                    display: flex; gap: 14px;
                    background: #FEFCF7;
                    border: 1px solid var(--line);
                    border-radius: 6px;
                    padding: 16px 18px;
                    transition: box-shadow 0.2s ease, transform 0.2s ease;
                }
                .book-card:hover { box-shadow: 0 12px 24px -14px rgba(58,50,41,0.3); transform: translateY(-2px); }

                .book-day { display: flex; flex-direction: column; align-items: center; gap: 4px; padding-top: 2px; min-width: 34px; }
                .day-num { font-family: 'Playfair Display', serif; font-size: 22px; color: var(--rose); font-weight: 600; line-height: 1; }
                .day-leaf svg { width: 16px; height: 16px; fill: var(--sage); }
                .day-leaf .vein { stroke: #FEFCF7; }

                .book-info { flex: 1; min-width: 0; }
                .book-title { font-family: 'Playfair Display', serif; font-size: 16.5px; font-weight: 600; color: var(--ink); line-height: 1.3; }
                .book-author { font-family: 'Jost', sans-serif; font-size: 12px; color: var(--ink-soft); margin-top: 3px; letter-spacing: 0.02em; }
                .book-rating { margin-top: 9px; display: flex; gap: 3px; align-items: center; }
                .bloom svg { width: 15px; height: 15px; fill: var(--line); }
                .bloom.filled svg { fill: var(--rose); }
                .bloom .bloom-center { fill: var(--paper); }
                .no-rating { font-family: 'Jost', sans-serif; font-size: 11.5px; color: var(--ink-soft); font-style: italic; }

                .empty-state { text-align: center; padding: 60px 20px; color: var(--ink-soft); }
                .empty-pot { width: 64px; height: 64px; margin-bottom: 14px; }
                .empty-pot .pot { fill: var(--rose); opacity: 0.35; }
                .empty-pot .pot-rim { fill: var(--gold); opacity: 0.5; }
                .empty-pot .stem { stroke: var(--evergreen-soft); stroke-width: 1.6; }
                .empty-pot path:not(.pot):not(.stem) { fill: var(--sage); }
                .empty-state p { font-family: 'EB Garamond', serif; font-size: 16px; font-style: italic; }

                .footer {
                    text-align: center;
                    margin-top: 56px;
                    padding-top: 22px;
                    border-top: 1px solid var(--line);
                    font-family: 'Jost', sans-serif;
                    font-size: 11px;
                    letter-spacing: 0.14em;
                    text-transform: uppercase;
                    color: var(--ink-soft);
                    display: flex; align-items: center; justify-content: center; gap: 8px;
                }
                .footer-sprig svg { width: 16px; height: 16px; }

                @media (max-width: 640px) {
                    body { padding: 24px 12px 60px; }
                    .container { padding: 36px 22px 32px; }
                    h1 { font-size: 28px; }
                    .books-grid { grid-template-columns: 1fr; }
                    .corner, .bg-flower { display: none; }
                }
            ";
        }

    }
}