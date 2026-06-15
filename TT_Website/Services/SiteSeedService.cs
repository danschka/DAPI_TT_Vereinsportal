using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class SiteSeedService
{
    private readonly AppDbContext _context;

    public SiteSeedService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(IWebHostEnvironment environment, IConfiguration configuration)
    {
        await SeedPagesAsync();
        await SeedTeamsAsync();
        await SeedSettingsAsync(configuration);
        await SeedSponsorsAsync();
        await SeedSampleGalleryGroupsAsync();
    }

    private async Task SeedPagesAsync()
    {
        var pages = new (string Title, string Slug, string? ParentSlug, int Sort, string? ExternalUrl)[]
        {
            ("Startseite", "startseite", null, 10, null),
            ("Über uns", "ueber-uns", null, 20, null),
            ("Vorstandschaft", "vorstandschaft", "ueber-uns", 10, null),
            ("Leitbild", "leitbild", "ueber-uns", 20, null),
            ("Chronik", "chronik", "ueber-uns", 30, null),
            ("Vereinslokal", "vereinslokal", "ueber-uns", 40, null),
            ("Aktionen", "aktionen", null, 30, null),
            ("Bogenberg Weihnachtsmarkt", "bogenberg-weihnachtsmarkt", "aktionen", 10, null),
            ("Tischtennis-Camps", "tischtennis-camps", "aktionen", 20, null),
            ("Jugendbildungsmaßnahmen", "jugendbildungsmassnahmen", "aktionen", 30, null),
            ("Schnuppermobil", "schnuppermobil", "aktionen", 40, null),
            ("Trikot Tag BLSV", "trikot-tag-blsv", "aktionen", 50, null),
            ("Auszeichnungen", "auszeichnungen", null, 40, null),
            ("Das grüne Band", "das-gruene-band", "auszeichnungen", 10, null),
            ("Quantensprung", "quantensprung", "auszeichnungen", 20, null),
            ("Sterne des Sports", "sterne-des-sports", "auszeichnungen", 30, null),
            ("Breitensportpreis", "breitensportpreis", "auszeichnungen", 40, null),
            ("Mitgliedschaft", "mitgliedschaft", null, 50, null),
            ("Mitgliederentwicklung", "mitgliederentwicklung", "mitgliedschaft", 10, null),
            ("Aufnahmeerklärung", "aufnahmeerklaerung", "mitgliedschaft", 20, null),
            ("Stammdatenänderung", "stammdatenaenderung", "mitgliedschaft", 30, null),
            ("Jugendarbeit", "jugendarbeit", null, 60, null),
            ("Trainingskonzept", "trainingskonzept", "jugendarbeit", 10, null),
            ("Schnuppern", "schnuppern", "jugendarbeit", 20, null),
            ("Sport nach 1", "sport-nach-1", "jugendarbeit", 30, null),
            ("TT-Sportabzeichen", "tt-sportabzeichen", "jugendarbeit", 40, null),
            ("Kooperation Schule & Verein", "kooperation-schule-verein", "jugendarbeit", 50, null),
            ("Impressionen", "impressionen", "jugendarbeit", 60, null),
            ("Mannschaften", "mannschaften-cms", null, 70, null),
            ("Mannschaftsübersicht", "mannschaftsuebersicht", "mannschaften-cms", 5, null),
            ("Vereinsrangliste", "vereinsrangliste", "mannschaften-cms", 10, null),
            ("myTischtennis", "mytischtennis", "mannschaften-cms", 30, "https://www.mytischtennis.de/"),
            ("Click-TT", "click-tt", "mannschaften-cms", 40, "https://www.bttv.de/click-tt"),
            ("Dokumente", "dokumente", null, 80, null),
            ("Training", "training", null, 90, null),
            ("News", "news", null, 100, null),
            ("Kontakt", "kontakt", null, 110, null),
            ("Sponsoren", "sponsoren", null, 120, null),
            ("Galerie", "galerie", null, 130, null),
            ("Links", "links", null, 140, null),
            ("Webshop", "webshop", null, 145, "https://de.butterfly.tt/"),
            ("Weihnachtsmarkt", "weihnachtsmarkt", null, 150, null),
            ("Impressum", "impressum", null, 160, null),
            ("Datenschutzerklärung", "datenschutz", null, 170, null)
        };

        var validSlugs = pages.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var outdatedPages = await _context.ContentPages
            .Where(x => !validSlugs.Contains(x.Slug))
            .ToListAsync();

        foreach (var outdatedPage in outdatedPages)
        {
            outdatedPage.IsActive = false;
            outdatedPage.ShowInNavigation = false;
        }

        foreach (var seed in pages.Where(x => x.ParentSlug is null))
        {
            await EnsurePageAsync(seed.Title, seed.Slug, null, seed.Sort, seed.ExternalUrl);
        }

        await _context.SaveChangesAsync();

        foreach (var seed in pages.Where(x => x.ParentSlug is not null))
        {
            var parent = await _context.ContentPages.FirstAsync(x => x.Slug == seed.ParentSlug);
            await EnsurePageAsync(seed.Title, seed.Slug, parent.Id, seed.Sort, seed.ExternalUrl);
        }

        var hiddenNavigationSlugs = new[]
        {
            "training",
            "news",
            "kontakt",
            "sponsoren",
            "galerie",
            "links",
            "weihnachtsmarkt",
            "impressum",
            "datenschutz"
        };

        var hiddenNavigationPages = await _context.ContentPages
            .Where(x => hiddenNavigationSlugs.Contains(x.Slug))
            .ToListAsync();

        foreach (var page in hiddenNavigationPages)
        {
            page.ShowInNavigation = false;
            page.IsActive = true;
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedTeamsAsync()
    {
        var teams = new TeamSeed[]
        {
            new("Damen 1", "Damen",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Damen_Bezirksoberliga_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492253/mannschaft/2951398/TSV_1883_Bogen_Tischtennis/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Damen_Bezirksoberliga_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492253/mannschaft/2951398/TSV_1883_Bogen_Tischtennis/spielplan/gesamt"),
            new("Damen 2", "Damen",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Damen_Bezirksoberliga_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492253/mannschaft/2949174/TSV_1883_Bogen_Tischtennis_II/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Damen_Bezirksoberliga_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492253/mannschaft/2949174/TSV_1883_Bogen_Tischtennis_II/spielplan/gesamt"),
            new("Herren 1", "Herren",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Bezirksklasse_A_Gruppe_1_Straubing/gruppe/492271/mannschaft/2951352/TSV_1883_Bogen_Tischtennis/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Bezirksklasse_A_Gruppe_1_Straubing/gruppe/492271/mannschaft/2951352/TSV_1883_Bogen_Tischtennis/spielplan/gesamt"),
            new("Herren 2", "Herren",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_A_Gruppe_1_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492271/mannschaft/2950159/TSV_1883_Bogen_Tischtennis_II/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_A_Gruppe_1_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492271/mannschaft/2950159/TSV_1883_Bogen_Tischtennis_II/spielplan/gesamt"),
            new("Herren 3", "Herren",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_B_Gruppe_1_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492307/mannschaft/2947986/TSV_1883_Bogen_Tischtennis_III/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_B_Gruppe_1_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492307/mannschaft/2947986/TSV_1883_Bogen_Tischtennis_III/spielplan/gesamt"),
            new("Herren 4", "Herren",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_B_Gruppe_1_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492307/mannschaft/2946795/TSV_1883_Bogen_Tischtennis_IV/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_B_Gruppe_1_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/492307/mannschaft/2946795/TSV_1883_Bogen_Tischtennis_IV/spielplan/gesamt"),
            new("Herren 5", "Herren",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_C_Gruppe_2_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/491960/mannschaft/2947207/TSV_1883_Bogen_Tischtennis_V/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_C_Gruppe_2_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/491960/mannschaft/2947207/TSV_1883_Bogen_Tischtennis_V/spielplan/gesamt"),
            new("Herren 6", "Herren",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_D_Gruppe_10_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/499963/mannschaft/2947254/TSV_1883_Bogen_Tischtennis_VI/spielerbilanzen/gesamt",
                "https://www.mytischtennis.de/click-tt/ByTTV/25--26/ligen/Erwachsene_Bezirksklasse_D_Gruppe_10_Straubing_(Bayerischer_TTV_-_Niederbayern-Ost)/gruppe/499963/mannschaft/2947254/TSV_1883_Bogen_Tischtennis_VI/spielplan/gesamt")
        };

        foreach (var seed in teams)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(x => x.Name == seed.Name);

            if (team is null)
            {
                team = new Team { Name = seed.Name };
                _context.Teams.Add(team);
            }

            team.Category = seed.Category;
            team.IsActive = true;
            team.MyTischtennisLeagueUrl = seed.StatisticsUrl;
            team.MyTischtennisStatisticsUrl = seed.StatisticsUrl;
            team.MyTischtennisScheduleUrl = seed.ScheduleUrl;
        }

        await _context.SaveChangesAsync();

        var youthTeams = new YouthTeamSeed[]
        {
            new("Jugend 1",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksoberliga_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/492902/mannschaft/2952229/TSV_1883_Bogen_Tischtennis/spielerbilanzen/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksoberliga_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/492902/mannschaft/2952229/TSV_1883_Bogen_Tischtennis/spielplan/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Landesliga_Osts%C3%BCdost_(Bayerischer_TTV)_R%C3%BCckrunde/gruppe/508117/mannschaft/3066382/TSV_1883_Bogen_Tischtennis/spielerbilanzen/rr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Landesliga_Osts%C3%BCdost_(Bayerischer_TTV)_R%C3%BCckrunde/gruppe/508117/mannschaft/3066382/TSV_1883_Bogen_Tischtennis/spielplan/rr"),
            new("Jugend 2",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksklasse_A_Gruppe_4_West_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/503938/mannschaft/2952792/TSV_1883_Bogen_Tischtennis_II/spielerbilanzen/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksklasse_A_Gruppe_4_West_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/503938/mannschaft/2952792/TSV_1883_Bogen_Tischtennis_II/spielplan/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_R%C3%BCckrunde/gruppe/508122/mannschaft/3065613/TSV_1883_Bogen_Tischtennis_II/spielerbilanzen/rr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_R%C3%BCckrunde/gruppe/508122/mannschaft/3065613/TSV_1883_Bogen_Tischtennis_II/spielplan/rr"),
            new("Jugend 3",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/492888/mannschaft/2952602/TSV_1883_Bogen_Tischtennis_III/spielerbilanzen/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/492888/mannschaft/2952602/TSV_1883_Bogen_Tischtennis_III/spielplan/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_R%C3%BCckrunde/gruppe/508122/mannschaft/3065254/TSV_1883_Bogen_Tischtennis_III/spielerbilanzen/rr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_R%C3%BCckrunde/gruppe/508122/mannschaft/3065254/TSV_1883_Bogen_Tischtennis_III/spielplan/rr"),
            new("Jugend 4",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/492888/mannschaft/2984699/TSV_1883_Bogen_Tischtennis_IV/spielerbilanzen/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JVR_25--26/jugend-punktspiele-vr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_Vorrunde/gruppe/492888/mannschaft/2984699/TSV_1883_Bogen_Tischtennis_IV/spielplan/vr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_R%C3%BCckrunde/gruppe/508122/mannschaft/3065688/TSV_1883_Bogen_Tischtennis_IV/spielerbilanzen/rr",
                "https://www.mytischtennis.de/click-tt/ByTTV/JRR_25--26/jugend-punktspiele-rr/Jugend_19_Bezirksklasse_B_Gruppe_2_Nord_(Bayerischer_TTV_-_Niederbayern-Ost)_R%C3%BCckrunde/gruppe/508122/mannschaft/3065688/TSV_1883_Bogen_Tischtennis_IV/spielplan/rr")
        };

        foreach (var seed in youthTeams)
        {
            var team = await _context.Teams
                .Include(x => x.Rounds)
                .FirstOrDefaultAsync(x => x.Name == seed.Name);

            if (team is null)
            {
                team = new Team { Name = seed.Name };
                _context.Teams.Add(team);
            }

            team.Category = "Jugend";
            team.IsActive = true;
            team.MyTischtennisLeagueUrl = null;
            team.MyTischtennisStatisticsUrl = null;
            team.MyTischtennisScheduleUrl = null;

            EnsureRound(team, "Vorrunde", seed.FirstRoundStatisticsUrl, seed.FirstRoundScheduleUrl);
            EnsureRound(team, "Rueckrunde", seed.SecondRoundStatisticsUrl, seed.SecondRoundScheduleUrl);
        }

        await _context.SaveChangesAsync();
    }

    private static void EnsureRound(Team team, string roundName, string statisticsUrl, string scheduleUrl)
    {
        var round = team.Rounds.FirstOrDefault(x => x.RoundName == roundName);

        if (round is null)
        {
            round = new TeamRound { RoundName = roundName };
            team.Rounds.Add(round);
        }

        round.MyTischtennisLeagueUrl = statisticsUrl;
        round.MyTischtennisStatisticsUrl = statisticsUrl;
        round.MyTischtennisScheduleUrl = scheduleUrl;
    }

    private async Task EnsurePageAsync(
        string title,
        string slug,
        int? parentId,
        int sortOrder,
        string? externalUrl)
    {
        var existing = await _context.ContentPages.FirstOrDefaultAsync(x => x.Slug == slug);

        if (existing is not null)
        {
            var seedContent = GetSeedContent(slug);

            existing.Title = title;
            existing.ParentId = parentId;
            existing.SortOrder = sortOrder;
            existing.ShowInNavigation = true;
            existing.IsActive = true;
            if (string.IsNullOrWhiteSpace(existing.ExternalUrl))
                existing.ExternalUrl = externalUrl;

            if (ShouldReplacePlaceholder(existing.Content) && seedContent is not null)
                existing.Content = seedContent.Value.Content;

            if (ShouldReplaceSummary(existing.Summary))
                existing.Summary = seedContent?.Summary;

            if (IsSpecialPublicPage(slug) && seedContent is not null)
            {
                if (ContainsMojibake(existing.Content))
                    existing.Content = seedContent.Value.Content;

                if (ContainsMojibake(existing.Summary))
                    existing.Summary = seedContent.Value.Summary;
            }

            if (slug == "vereinslokal")
            {
                existing.ShowMap = true;
                existing.MapAddress ??= "Niedermenach 3, 94327 Bogen";
            }

            return;
        }

        var newSeedContent = GetSeedContent(slug);

        _context.ContentPages.Add(new ContentPage
        {
            Title = title,
            Slug = slug,
            ParentId = parentId,
            SortOrder = sortOrder,
            ShowInNavigation = true,
            IsActive = true,
            ExternalUrl = externalUrl,
            ShowMap = slug == "vereinslokal",
            MapAddress = slug == "vereinslokal" ? "Niedermenach 3, 94327 Bogen" : null,
            Content = newSeedContent?.Content ?? "",
            Summary = newSeedContent?.Summary
        });
    }

    private static bool ShouldReplacePlaceholder(string? content)
    {
        return string.IsNullOrWhiteSpace(content) ||
            content.StartsWith("Hier kann später der Inhalt", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldReplaceSummary(string? summary)
    {
        return string.IsNullOrWhiteSpace(summary) ||
            summary.Contains("Adminbereich bearbeitet", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsMojibake(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            (value.Contains('Ã') || value.Contains('Â'));
    }

    private static bool IsSpecialPublicPage(string slug)
    {
        return slug is
            "training" or
            "news" or
            "kontakt" or
            "sponsoren" or
            "galerie" or
            "links" or
            "webshop" or
            "weihnachtsmarkt" or
            "impressum" or
            "datenschutz";
    }

    private static (string Summary, string Content)? GetSeedContent(string slug)
    {
        return slug switch
        {
            "ueber-uns" => (
                "Der TSV 1883 Bogen Tischtennis steht für Sport, Jugendarbeit und ein aktives Vereinsleben.",
                """
                Der Verein wurde als Abteilung des TSV 1883 Bogen Hauptverein im Jahr 1959 gegründet und im Jahr 2014 als eigenständiger eingetragener Verein verselbständigt.

                Die Mitgliederanzahl liegt insgesamt bei 170, davon 64 aktive Mitglieder. Der Rest sind Fördermitglieder.

                In den Verbandsspielen wurden bisher maximal 11 Mannschaften gemeldet, davon eine Damen-, vier Jugend- und sechs Herrenmannschaften.

                Aus sportlicher Sicht waren die höchsten Ligen der Jugendmannschaft die Verbandsliga Südost (Bayernliga), bei Damen und Herren die Bezirksoberliga.

                Die Jugendarbeit ist ein wichtiger Teil der Vereinsarbeit. Im Schnitt sind 25 aktive Nachwuchsspielerinnen und Nachwuchsspieler im Training und werden von sechs ausgebildeten Übungsleitern mit C-Trainerschein betreut.

                Trainingstage sind Dienstag und Freitag. Das Jugendtraining beginnt jeweils ab 17:30 Uhr.

                In der Zweifachturnhalle der Herzog-Ludwig-Mittelschule Bogen stehen 10 blaue Tischtennisplatten zur Verfügung.

                Veranstaltungen wie Radausflüge, Grillfeste, Weihnachtsmarkt Bogenberg, Jahresabschlussfeier, Jugendbildungsmaßnahmen, Zeltlager, Tischtennis-Camps und Skifahrten runden den sportlichen Bereich ab.

                Aktionen wie Breitensportpreise, Quantensprung, Das grüne Band, DOSB Sterne des Sports, Kooperation Schule und Verein, Ferienfreizeit Stadt Bogen, Sportabzeichen und Sport nach 1 werden regelmäßig angeboten oder durchgeführt.
                """),
            "leitbild" => (
                "Unser Verein steht für Gemeinschaft, Fairness, Respekt und Freude am Tischtennissport.",
                """
                Wer sind wir?
                Wir sind ein engagierter und familiärer Verein, bei dem der Mensch im Mittelpunkt steht. Unabhängig von Alter, Herkunft oder Leistungsniveau möchten wir allen Mitgliedern eine sportliche Heimat bieten. Unsere große Sport-Familie lebt von Gemeinschaft, gegenseitigem Respekt und dem gemeinsamen Interesse am Tischtennissport.

                Was tun wir?
                Wir fördern Kinder, Jugendliche und Erwachsene entsprechend ihrer individuellen Fähigkeiten, Talente und Leistungsbereitschaft. Dabei legen wir großen Wert auf eine gute sportliche Ausbildung, persönliche Entwicklung und Freude am Sport.

                Was wollen wir erreichen?
                Unser Ziel ist es, jedem Mitglied die Möglichkeit zu geben, sein persönliches Potential bestmöglich zu entfalten. Wir möchten Begeisterung für den Tischtennissport vermitteln, Talente fördern und Werte wie Teamgeist, Fairness und Verantwortungsbewusstsein stärken.

                Wie sehen wir unser Miteinander?
                Ein respektvoller und freundschaftlicher Umgang miteinander ist für uns selbstverständlich. Nach dem Motto Einer für Alle, Alle für Einen unterstützen wir uns gegenseitig und wachsen als Gemeinschaft zusammen.

                Was zeichnet uns aus?
                Wir stehen für Fairness, Respekt und Toleranz. Gewalt, Diskriminierung und Rassismus haben in unserem Verein keinen Platz. Offenheit, gegenseitige Wertschätzung und ein positives Miteinander prägen unseren Verein.

                Wie definieren wir Führungsarbeit?
                Ehrenamtliches Engagement ist eine wichtige Grundlage unseres Vereins. Trainer, Betreuer, Vorstandschaft und Helfer engagieren sich mit Leidenschaft für den Verein.

                Wie treffen wir Entscheidungen?
                Grundlegende Entscheidungen treffen wir gemeinsam, konstruktiv und sachlich. Offenheit, Fairness und gegenseitiges Vertrauen bilden die Basis unserer Vereinsarbeit.
                """),
            "vereinslokal" => (
                "Unser Vereinslokal bei Anna in Niedermenach ist Treffpunkt für Mitglieder, Gäste und Familien.",
                """
                Das Vereinslokal bei Anna in Niedermenach wird von der Familie Nikomanis geführt und bietet eine gemütliche Atmosphäre für Vereinsmitglieder, Gäste und Familien.

                Mit griechischen und traditionellen Spezialitäten sowie herzlicher Gastfreundschaft ist das Lokal ein beliebter Treffpunkt für gemeinsame Veranstaltungen und gesellige Abende.

                Restaurant bei Anna
                Niedermenach 3
                94327 Bogen
                """),
            "bogenberg-weihnachtsmarkt" => (
                "Der TSV 1883 Bogen Tischtennis engagiert sich jedes Jahr am Bogenberger Weihnachtsmarkt.",
                """
                Der TSV 1883 Bogen Tischtennis e.V. engagiert sich jedes Jahr mit großer Freude am traditionellen Weihnachtsmarkt am Bogenberg und trägt damit aktiv zum festlichen Vereins- und Gemeindeleben bei.

                Mit viel Einsatz und Unterstützung zahlreicher Helferinnen und Helfer betreibt der Verein dort einen eigenen Stand, der aus zwei liebevoll gestalteten Verkaufshütten besteht.

                An der Getränkehütte werden warme Getränke wie Glühwein, Kinderpunsch und weitere winterliche Getränke angeboten. In der Verpflegungshütte gibt es herzhafte Spezialitäten wie Rosswurstsemmeln und frisch zubereitete Rollbratensemmeln.

                Der Weihnachtsmarkt am Bogenberg ist für den Verein ein besonderes Highlight im Jahreskalender. Gemeinschaft, Zusammenhalt und geselliges Miteinander stehen dabei im Mittelpunkt.
                """),
            "schnuppern" => (
                "Schnuppertraining ist ganzjährig möglich und richtet sich an Kinder, Jugendliche und Erwachsene.",
                """
                In Zusammenarbeit mit dem Deutschen Tischtennis Bund organisieren wir regelmäßig Tischtennis-Schnupperkurse. Die Teilnehmer werden spielerisch an die Sportart Tischtennis herangeführt.

                Voraussetzung für die Teilnahme ist nur Spaß an Sport und Bewegung mit dem Ball. Spielerische Erfahrungen oder Talent sind zunächst nebensächlich.

                Außerdem bieten wir das ganze Jahr Schnuppertraining für Jung und Alt an. Kommt einfach zu unseren Trainingszeiten in Sportkleidung vorbei oder kontaktiert unsere Ansprechpartner.

                Tischtennisschläger sind leihweise erhältlich. Eine Mitgliedschaft ist für das Schnuppertraining nicht erforderlich.
                """),
            "mitgliedschaft" => (
                "Hier findest du die wichtigsten Informationen zu den Mitgliedsarten im Verein.",
                """
                Aktive Mitglieder
                Aktive Vereinsmitglieder bringen ihre Arbeitskraft und Ideen ein, gestalten die Vereinsarbeit mit und nehmen in der Regel an Veranstaltungen, Wettbewerben, Turnieren und am Training teil. Sie können mit Stimmrecht an der Mitgliederversammlung teilnehmen.

                Passive Mitglieder
                Passive Mitglieder nehmen an keinen Verbandswettbewerben teil, können jedoch am Training teilnehmen. Sie können mit Stimmrecht an der Mitgliederversammlung teilnehmen und den Verein unterstützen.

                Ehrenmitglieder
                Mitglieder, die sich um den Verein oder den Sport im Allgemeinen sehr verdient gemacht haben, können durch Beschluss des Vorstandes zum Ehrenmitglied ernannt werden.

                Fördermitglieder
                Fördernde Mitglieder beteiligen sich nicht aktiv innerhalb des Vereins, unterstützen jedoch die Ziele und den Zweck des Vereins durch regelmäßige oder unregelmäßige Beiträge, Sachleistungen oder Dienstleistungen.
                """),
            "stammdatenaenderung" => (
                "Teile uns Änderungen an Adresse, Kontakt- oder Bankdaten direkt online mit.",
                """
                Wenn sich persönliche Daten wie Adresse, Telefonnummer, E-Mail-Adresse oder Bankverbindung ändern, bitten wir unsere Mitglieder, uns dies zeitnah mitzuteilen. Nur so können wir eine reibungslose Kommunikation sowie eine korrekte Verwaltung der Mitgliedsdaten gewährleisten.

                Nutze dafür bitte das Formular auf dieser Seite. Die Angaben werden automatisch per E-Mail an den Verein gesendet.
                """),
            "mitgliederentwicklung" => (
                "Die Mitgliederentwicklung zeigt die Entwicklung des Vereins seit der Verselbständigung.",
                """
                Die Mitgliederentwicklung zeigt die Entwicklung des Vereins seit der Verselbständigung. Aktuell zählt der Verein rund 170 Mitglieder, davon 64 aktive Mitglieder.
                """),
            "vereinsrangliste" => (
                "Die Vereinsrangliste verweist auf die aktuelle andro-Rangliste von myTischtennis.",
                """
                Die Vereinsrangliste wird auf Basis der andro-Rangliste von myTischtennis geführt und automatisch auf dieser Seite angezeigt.
                """),
            "training" => (
                "Jugend, Erwachsene und freies Training finden regelmäßig in der Zweifachturnhalle der Herzog-Ludwig-Mittelschule Bogen statt.",
                """
                Wann trainiert die Jugend?
                Dienstag und Freitag, jeweils ab 17:30 Uhr.

                Wann trainieren Erwachsene?
                Dienstag und Freitag, jeweils ab 19:00 Uhr.

                Wo findet das Training statt?
                In der Zweifachturnhalle der Herzog-Ludwig-Mittelschule Bogen stehen 10 blaue Tischtennisplatten zur Verfügung.
                """),
            "news" => (
                "Neuigkeiten aus Training, Spielbetrieb, Vereinsleben und Jugendbereich.",
                ""),
            "kontakt" => (
                "Fragen zum Training, Probetraining oder zur Mitgliedschaft beantworten wir unkompliziert.",
                """
                Wie erreichst du uns?
                Schreib uns, wenn du beim Training reinschnuppern möchtest oder Informationen zum Verein brauchst. Neue Spielerinnen und Spieler sind herzlich willkommen.
                """),
            "sponsoren" => (
                "Diese Unternehmen unterstützen unseren Verein und das Tischtennis in Bogen.",
                ""),
            "galerie" => (
                "Bilder aus Training, Spielbetrieb, Aktionen und Vereinsleben.",
                ""),
            "links" => (
                "Social Media, Webshop und weitere wichtige Anlaufstellen.",
                """
                Wo findest du uns online?
                Hier findest du unsere Social-Media-Kanäle und weitere wichtige Links.

                Wo gibt es Vereinskleidung?
                Vereinskleidung und Zubehör findest du in unserem Webshop.
                """),
            "weihnachtsmarkt" => (
                "Eindrücke und Bilder rund um den Weihnachtsmarkt des Vereins.",
                """
                Was macht den Weihnachtsmarkt besonders?
                Der Weihnachtsmarkt ist ein fester Treffpunkt im Vereinsjahr. Mitglieder, Familien und Gäste kommen zusammen, helfen mit und erleben den Verein auch abseits der Sporthalle.

                Was gibt es zu sehen?
                Hier sammeln wir Eindrücke, Bilder und Erinnerungen rund um den Weihnachtsmarkt und die gemeinsamen Aktionen des TSV 1883 Bogen Tischtennis.
                """),
            "impressum" => (
                "Rechtliche Angaben zum TSV 1883 Bogen Tischtennis.",
                ""),
            "datenschutz" => (
                "Informationen zur Verarbeitung personenbezogener Daten auf dieser Website.",
                ""),
            _ => null
        };
    }

    private async Task SeedSettingsAsync(IConfiguration configuration)
    {
        if (!await _context.SiteSettings.AnyAsync(x => x.Key == SiteSettingsService.MemberApplicationRecipientEmail))
        {
            var configuredEmail = configuration["EmailSettings:ToEmail"] ?? "";

            _context.SiteSettings.Add(new SiteSetting
            {
                Key = SiteSettingsService.MemberApplicationRecipientEmail,
                Value = IsValidEmail(configuredEmail) ? configuredEmail : "",
                UpdatedAt = DateTime.Now
            });
        }

        if (!await _context.SiteSettings.AnyAsync(x => x.Key == SiteSettingsService.RankingUrl))
        {
            _context.SiteSettings.Add(new SiteSetting
            {
                Key = SiteSettingsService.RankingUrl,
                Value = "https://www.mytischtennis.de/rankings/andro-rangliste?clubnr=415010&fednickname=ByTTV&all-players=on&continent=all&country=all",
                UpdatedAt = DateTime.Now
            });
        }

        if (!await _context.SiteSettings.AnyAsync(x => x.Key == SiteSettingsService.MemberDevelopmentData))
        {
            _context.SiteSettings.Add(new SiteSetting
            {
                Key = SiteSettingsService.MemberDevelopmentData,
                Value = System.Text.Json.JsonSerializer.Serialize(
                    SiteSettingsService.GetDefaultMemberDevelopment(),
                    new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)),
                UpdatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task SeedSponsorsAsync()
    {
        var sponsors = new (string Name, string LogoPath, string? WebsiteUrl)[]
        {
            ("Borowiak Rechtsanwälte", "/uploads/sponsors/borowiak-rechtsanwaelte.jpg", null),
            ("Hubertus Apotheke", "/uploads/sponsors/hubertus-apotheke.jpg", null),
            ("meine Reiselounge Bogen", "/uploads/sponsors/meine-reiselounge.png", null),
            ("Physio-Zentrum Bogen", "/uploads/sponsors/physio-zentrum-bogen.jpg", null),
            ("venus werbeagentur gmbh", "/uploads/sponsors/venus.png", null),
            ("Volksbank Straubing", "/uploads/sponsors/volksbank-straubing.png", null),
            ("Zahnarztpraxis Dr. Huber", "/uploads/sponsors/zahnarzt-dr-huber.jpg", null)
        };

        foreach (var seed in sponsors)
        {
            var sponsor = await _context.Sponsors
                .FirstOrDefaultAsync(x => x.Name == seed.Name);

            if (sponsor is null)
            {
                _context.Sponsors.Add(new Sponsor
                {
                    Name = seed.Name,
                    LogoPath = seed.LogoPath,
                    WebsiteUrl = seed.WebsiteUrl,
                    IsActive = true
                });

                continue;
            }

            sponsor.LogoPath = seed.LogoPath;
            sponsor.WebsiteUrl = seed.WebsiteUrl ?? sponsor.WebsiteUrl;
            sponsor.IsActive = true;
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedSampleGalleryGroupsAsync()
    {
        var images = await _context.GalleryImages.ToListAsync();

        if (images.Count == 0)
            return;

        var groups = new[]
        {
            ("Vereinsleben", "Bilder aus Training, Aktionen und Vereinsleben.", "startseite"),
            ("Jugendarbeit", "Impressionen aus Jugendtraining und Nachwuchsarbeit.", "jugendarbeit"),
            ("Mannschaften", "Beispielbilder für Mannschaften und Spielbetrieb.", "mannschaften-cms")
        };

        foreach (var seed in groups)
        {
            var group = await _context.GalleryGroups
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Name == seed.Item1);

            if (group is null)
            {
                group = new GalleryGroup
                {
                    Name = seed.Item1,
                    Description = seed.Item2
                };

                _context.GalleryGroups.Add(group);
                await _context.SaveChangesAsync();
            }

            if (group.Images.Count == 0)
            {
                var order = 1;
                foreach (var image in images.Take(6))
                {
                    _context.GalleryGroupImages.Add(new GalleryGroupImage
                    {
                        GalleryGroupId = group.Id,
                        GalleryImageId = image.Id,
                        SortOrder = order++
                    });
                }
            }

            var page = await _context.ContentPages.FirstOrDefaultAsync(x => x.Slug == seed.Item3);
            if (page is not null)
            {
                var assigned = await _context.ContentPageGalleryGroups
                    .AnyAsync(x => x.ContentPageId == page.Id && x.GalleryGroupId == group.Id);

                if (!assigned)
                {
                    _context.ContentPageGalleryGroups.Add(new ContentPageGalleryGroup
                    {
                        ContentPageId = page.Id,
                        GalleryGroupId = group.Id,
                        SortOrder = 1
                    });
                }
            }

            if (seed.Item3 == "startseite" &&
                !await _context.PageGalleryAssignments.AnyAsync(x => x.PageKey == "home"))
            {
                _context.PageGalleryAssignments.Add(new PageGalleryAssignment
                {
                    PageKey = "home",
                    PageTitle = "Startseite",
                    GalleryGroupId = group.Id,
                    SlideshowEnabled = true,
                    IntervalSeconds = 3
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private sealed record TeamSeed(
        string Name,
        string Category,
        string StatisticsUrl,
        string ScheduleUrl);

    private sealed record YouthTeamSeed(
        string Name,
        string FirstRoundStatisticsUrl,
        string FirstRoundScheduleUrl,
        string SecondRoundStatisticsUrl,
        string SecondRoundScheduleUrl);
}
