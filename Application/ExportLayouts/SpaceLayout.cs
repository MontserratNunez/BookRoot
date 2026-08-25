using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Application.ExportLayouts
{
    public static class SpaceLayout
    {
        
        private static readonly string[] PlanetHues = { "#7C6CF0", "#46D9CF", "#F5C56B", "#E8749C", "#5CA8F0", "#8FE08A" };

        public static string Generate(
            string username,
            IEnumerable<IGrouping<int, Domain.Entities.Interaction>> grouped,
            Dictionary<string, Domain.Entities.BookMetadata> bookDict,
            string filterLabel,
            int totalBooks)
        {
            var monthNames = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
            var booksHtml = new StringBuilder();
            int planetIndex = 0;

            foreach (var yearGroup in grouped)
            {
                booksHtml.Append($@"
                <section class='year-block'>
                    <div class='year-heading'>
                        <span class='year-orbit-dot'></span>
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

                    var hue = PlanetHues[planetIndex % PlanetHues.Length];
                    planetIndex++;

                    booksHtml.Append($@"
                    <div class='month-block'>
                        <div class='month-header'>
                            <span class='planet-marker' style='--planet-color:{hue}'>{PlanetIconSmall}</span>
                            <h3 class='month-title'>{WebUtility.HtmlEncode(monthName)}</h3>
                            <span class='month-count'>{monthBooks.Count} {(monthBooks.Count == 1 ? "registro" : "registros")}</span>
                        </div>
                        <div class='constellation-line' style='--planet-color:{hue}'></div>
                        <div class='books-grid'>");

                    foreach (var interaction in monthBooks)
                    {
                        var book = bookDict[interaction.BookId];
                        var day = interaction.FinishedAt?.ToString("dd");
                        var ratingHtml = BuildRatingStars(interaction.Rating);

                        booksHtml.Append($@"
                            <article class='book-card'>
                                <div class='book-node' style='--planet-color:{hue}'>
                                    <span class='node-dot'></span>
                                    <span class='node-day'>{day}</span>
                                </div>
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
                    {EmptyTelescopeIcon}
                    <p>Ningún registro capturado en este período. El observatorio espera.</p>
                </div>");
            }

            var css = BuildCss();

            return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Bitácora Estelar — {WebUtility.HtmlEncode(username)}</title>
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link href=""https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;600;700&family=Inter:wght@300;400;500;600&family=JetBrains+Mono:wght@400;500&display=swap"" rel=""stylesheet"">
    <style>{css}</style>
</head>
<body>
    <div class='starfield'></div>
    <div class='planet planet-tl'>{PlanetRinged}</div>
    <div class='planet planet-br'>{PlanetRinged}</div>

    <div class='container'>
        <header class='header'>
            <div class='header-badge'>{OrbitIcon}<span>Bitácora Estelar</span></div>
            <h1>Constelación de Lecturas de <span>{WebUtility.HtmlEncode(username)}</span></h1>
            <div class='meta'>{WebUtility.HtmlEncode(filterLabel)} &middot; Generado el {DateTime.Now:dd 'de' MMMM 'de' yyyy}</div>
        </header>

        <div class='stats-bar'>
            <div class='stat-item'>
                <span class='stat-icon'>{StarBurstIcon}</span>
                <div class='stat-num'>{totalBooks}</div>
                <div class='stat-label'>{(totalBooks == 1 ? "Planeta Visitado" : "Planetas Visitados")}</div>
            </div>
        </div>

        <div class='timeline'>
            {booksHtml}
        </div>

        <footer class='footer'>
            <span>Creado con BookRoot</span>
            <span class='dot-sep'>&bull;</span>
            <span>bookroot.net</span>
        </footer>
    </div>
</body>
</html>";
        }

        private static string BuildRatingStars(int? rating)
        {
            if (!rating.HasValue)
            {
                return "<span class='no-rating'>Sin calificación</span>";
            }

            var sb = new StringBuilder();
            for (int i = 1; i <= 5; i++)
            {
                var filled = i <= rating.Value;
                sb.Append($"<span class='star {(filled ? "filled" : "")}'>{StarIcon}</span>");
            }
            return sb.ToString();
        }

        private const string StarIcon = @"<svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg'>
            <path d='M12 2 L14.6 9.2 L22 9.9 L16.4 14.9 L18.2 22 L12 18 L5.8 22 L7.6 14.9 L2 9.9 L9.4 9.2 Z'/>
        </svg>";

        private const string PlanetIconSmall = @"<svg viewBox='0 0 32 32' xmlns='http://www.w3.org/2000/svg'>
            <circle cx='16' cy='16' r='9' class='planet-body'/>
            <ellipse cx='16' cy='16' rx='15' ry='4.2' class='planet-ring' transform='rotate(-18 16 16)' fill='none'/>
        </svg>";

        private static readonly string PlanetRinged = @"<svg viewBox='0 0 220 220' xmlns='http://www.w3.org/2000/svg'>
            <defs>
                <radialGradient id='pg' cx='35%' cy='30%' r='75%'>
                    <stop offset='0%' stop-color='#B9AEFF'/>
                    <stop offset='55%' stop-color='#7C6CF0'/>
                    <stop offset='100%' stop-color='#463A9E'/>
                </radialGradient>
            </defs>
            <ellipse cx='110' cy='110' rx='104' ry='24' fill='none' stroke='#46D9CF' stroke-width='2.5' opacity='0.55' transform='rotate(-20 110 110)'/>
            <circle cx='110' cy='110' r='58' fill='url(#pg)'/>
            <ellipse cx='110' cy='110' rx='104' ry='24' fill='none' stroke='#F5C56B' stroke-width='2' opacity='0.35' transform='rotate(-20 110 110)' style='clip-path:inset(0 0 50% 0)'/>
        </svg>";

        private const string OrbitIcon = @"<svg viewBox='0 0 24 24' xmlns='http://www.w3.org/2000/svg' class='orbit-icon'>
            <circle cx='12' cy='12' r='2.4' class='sun'/>
            <ellipse cx='12' cy='12' rx='10' ry='4.4' fill='none' stroke-width='1.3'/>
        </svg>";

        private const string StarBurstIcon = @"<svg viewBox='0 0 40 40' xmlns='http://www.w3.org/2000/svg'>
            <path d='M20 2 L23.6 15.8 L38 17 L26.4 25.6 L29.2 40 L20 32 L10.8 40 L13.6 25.6 L2 17 L16.4 15.8 Z'/>
        </svg>";

        private const string EmptyTelescopeIcon = @"<svg viewBox='0 0 64 64' xmlns='http://www.w3.org/2000/svg' class='empty-telescope'>
            <rect x='10' y='34' width='34' height='9' rx='4' transform='rotate(-24 10 34)' class='tube'/>
            <circle cx='44' cy='44' r='7' class='lens'/>
            <path d='M20 46 L14 58 M28 50 L26 60' class='legs' stroke-width='2.4' stroke-linecap='round'/>
        </svg>";

        private static string BuildCss()
        {
            return @"
                :root {
                    --void: #060814;
                    --deep: #0C1130;
                    --panel: #10163C;
                    --nebula: #7C6CF0;
                    --aqua: #46D9CF;
                    --gold: #F5C56B;
                    --text: #E9E8F7;
                    --muted: #9098C8;
                    --line: rgba(233, 232, 247, 0.1);
                }

                * { box-sizing: border-box; margin: 0; padding: 0; }

                body {
                    background:
                        radial-gradient(ellipse at 20% -10%, #241E56 0%, transparent 55%),
                        radial-gradient(ellipse at 90% 110%, #12203E 0%, transparent 50%),
                        var(--void);
                    font-family: 'Inter', sans-serif;
                    color: var(--text);
                    padding: 48px 24px 90px;
                    position: relative;
                    overflow-x: hidden;
                    min-height: 100vh;
                }

                .starfield {
                    position: fixed; inset: 0; z-index: 0; pointer-events: none;
                    background-image:
                        radial-gradient(1.6px 1.6px at 12% 18%, #fff 100%, transparent 100%),
                        radial-gradient(1.2px 1.2px at 28% 64%, #fff 100%, transparent 100%),
                        radial-gradient(1.8px 1.8px at 46% 22%, #fff 100%, transparent 100%),
                        radial-gradient(1.2px 1.2px at 64% 78%, #fff 100%, transparent 100%),
                        radial-gradient(1.6px 1.6px at 78% 34%, #fff 100%, transparent 100%),
                        radial-gradient(1.2px 1.2px at 88% 62%, #fff 100%, transparent 100%),
                        radial-gradient(1.4px 1.4px at 6% 82%, #fff 100%, transparent 100%),
                        radial-gradient(1.4px 1.4px at 55% 92%, #fff 100%, transparent 100%),
                        radial-gradient(1.2px 1.2px at 35% 8%, #fff 100%, transparent 100%),
                        radial-gradient(1.6px 1.6px at 95% 12%, #fff 100%, transparent 100%);
                    background-repeat: repeat;
                    background-size: 100% 100%;
                    opacity: 0.55;
                }

                .planet { position: fixed; width: 190px; height: 190px; z-index: 0; pointer-events: none; opacity: 0.85; filter: drop-shadow(0 0 40px rgba(124,108,240,0.35)); }
                .planet-tl { top: -50px; left: -50px; }
                .planet-br { bottom: -60px; right: -60px; }

                .container {
                    max-width: 880px;
                    margin: 0 auto;
                    background: rgba(16, 22, 60, 0.72);
                    backdrop-filter: blur(6px);
                    border: 1px solid var(--line);
                    border-radius: 10px;
                    box-shadow: 0 40px 90px -30px rgba(0,0,0,0.6), inset 0 1px 0 rgba(255,255,255,0.04);
                    padding: 56px 60px 48px;
                    position: relative;
                    z-index: 1;
                }

                .header { text-align: center; padding-bottom: 32px; border-bottom: 1px solid var(--line); }
                .header-badge {
                    display: inline-flex; align-items: center; gap: 8px;
                    font-family: 'JetBrains Mono', monospace;
                    letter-spacing: 0.22em;
                    text-transform: uppercase;
                    font-size: 11px;
                    color: var(--aqua);
                    margin-bottom: 18px;
                }
                .orbit-icon { width: 18px; height: 18px; fill: none; stroke: var(--aqua); }
                .orbit-icon .sun { fill: var(--gold); stroke: none; }
                h1 {
                    font-family: 'Space Grotesk', sans-serif;
                    font-weight: 600;
                    font-size: 38px;
                    color: var(--text);
                    line-height: 1.3;
                }
                h1 span {
                    background: linear-gradient(90deg, var(--nebula), var(--aqua));
                    -webkit-background-clip: text;
                    background-clip: text;
                    color: transparent;
                }
                .meta {
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 12.5px;
                    color: var(--muted);
                    margin-top: 14px;
                    text-transform: capitalize;
                    letter-spacing: 0.02em;
                }

                .stats-bar { display: flex; justify-content: center; padding: 36px 0 10px; }
                .stat-item { text-align: center; }
                .stat-icon svg { width: 38px; height: 38px; fill: var(--gold); filter: drop-shadow(0 0 8px rgba(245,197,107,0.6)); margin-bottom: 8px; }
                .stat-num { font-family: 'Space Grotesk', sans-serif; font-size: 44px; font-weight: 700; color: var(--text); line-height: 1; }
                .stat-label {
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 11px;
                    letter-spacing: 0.14em;
                    text-transform: uppercase;
                    color: var(--muted);
                    margin-top: 8px;
                }

                .timeline { margin-top: 26px; }

                .year-block { margin-top: 46px; }
                .year-block:first-child { margin-top: 30px; }
                .year-heading { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
                .year-orbit-dot { width: 10px; height: 10px; border-radius: 50%; background: var(--nebula); box-shadow: 0 0 12px var(--nebula); flex-shrink: 0; }
                .year-title { font-family: 'Space Grotesk', sans-serif; font-size: 25px; color: var(--text); font-weight: 600; }
                .year-line { flex: 1; height: 1px; background: linear-gradient(90deg, var(--line), transparent); }

                .month-block { position: relative; margin-top: 30px; }
                .month-header { display: flex; align-items: center; gap: 10px; margin-bottom: 4px; }
                .planet-marker svg { width: 24px; height: 24px; }
                .planet-marker .planet-body { fill: var(--planet-color); }
                .planet-marker .planet-ring { stroke: var(--planet-color); opacity: 0.55; }
                .month-title {
                    font-family: 'Space Grotesk', sans-serif;
                    font-weight: 600;
                    font-size: 13px;
                    letter-spacing: 0.18em;
                    text-transform: uppercase;
                    color: var(--text);
                }
                .month-count { font-family: 'JetBrains Mono', monospace; font-size: 11.5px; color: var(--muted); margin-left: auto; }
                .constellation-line { height: 1px; margin: 12px 0 16px 34px; background: linear-gradient(90deg, var(--planet-color), transparent); opacity: 0.5; }

                .books-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 14px; padding-left: 4px; }

                .book-card {
                    display: flex; gap: 14px;
                    background: rgba(233,232,247,0.03);
                    border: 1px solid var(--line);
                    border-radius: 8px;
                    padding: 16px 18px;
                    transition: box-shadow 0.2s ease, transform 0.2s ease, border-color 0.2s ease;
                }
                .book-card:hover { box-shadow: 0 0 0 1px var(--planet-color, var(--nebula)), 0 16px 32px -18px rgba(124,108,240,0.5); transform: translateY(-2px); }

                .book-node { display: flex; flex-direction: column; align-items: center; gap: 6px; min-width: 32px; padding-top: 3px; }
                .node-dot { width: 8px; height: 8px; border-radius: 50%; background: var(--planet-color, var(--aqua)); box-shadow: 0 0 10px var(--planet-color, var(--aqua)); }
                .node-day { font-family: 'JetBrains Mono', monospace; font-size: 13px; color: var(--muted); }

                .book-info { flex: 1; min-width: 0; }
                .book-title { font-family: 'Space Grotesk', sans-serif; font-size: 15.5px; font-weight: 600; color: var(--text); line-height: 1.35; }
                .book-author { font-family: 'Inter', sans-serif; font-size: 12px; color: var(--muted); margin-top: 4px; }
                .book-rating { margin-top: 10px; display: flex; gap: 3px; align-items: center; }
                .star svg { width: 13px; height: 13px; fill: rgba(233,232,247,0.15); }
                .star.filled svg { fill: var(--gold); filter: drop-shadow(0 0 4px rgba(245,197,107,0.7)); }
                .no-rating { font-family: 'Inter', sans-serif; font-size: 11.5px; color: var(--muted); font-style: italic; }

                .empty-state { text-align: center; padding: 60px 20px; color: var(--muted); }
                .empty-telescope { width: 60px; height: 60px; margin-bottom: 16px; }
                .empty-telescope .tube { fill: var(--nebula); opacity: 0.5; }
                .empty-telescope .lens { fill: none; stroke: var(--aqua); stroke-width: 2; }
                .empty-telescope .legs { stroke: var(--muted); fill: none; }
                .empty-state p { font-family: 'Inter', sans-serif; font-size: 15px; }

                .footer {
                    text-align: center;
                    margin-top: 56px;
                    padding-top: 22px;
                    border-top: 1px solid var(--line);
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 10.5px;
                    letter-spacing: 0.12em;
                    text-transform: uppercase;
                    color: var(--muted);
                }
                .dot-sep { margin: 0 8px; color: var(--nebula); }

                @media (max-width: 640px) {
                    body { padding: 24px 12px 60px; }
                    .container { padding: 36px 22px 32px; }
                    h1 { font-size: 26px; }
                    .books-grid { grid-template-columns: 1fr; }
                    .planet { display: none; }
                }
            ";
        }
    }
}