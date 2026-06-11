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
            ("Statistik", "statistik", "mannschaften-cms", 20, null),
            ("myTischtennis", "mytischtennis", "mannschaften-cms", 30, "https://www.mytischtennis.de/"),
            ("Click-TT", "click-tt", "mannschaften-cms", 40, "https://www.bttv.de/click-tt")
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
            existing.Title = title;
            existing.ParentId = parentId;
            existing.SortOrder = sortOrder;
            existing.ShowInNavigation = true;
            existing.IsActive = true;
            existing.ExternalUrl = externalUrl;

            if (slug == "vereinslokal")
            {
                existing.ShowMap = true;
                existing.MapAddress ??= "Bogen, Deutschland";
            }

            return;
        }

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
            MapAddress = slug == "vereinslokal" ? "Bogen, Deutschland" : null,
            Content = $"Hier kann später der Inhalt für \"{title}\" gepflegt werden.",
            Summary = "Text und Bilder können im Adminbereich bearbeitet werden."
        });
    }

    private async Task SeedSettingsAsync(IConfiguration configuration)
    {
        if (await _context.SiteSettings.AnyAsync(x => x.Key == SiteSettingsService.MemberApplicationRecipientEmail))
            return;

        _context.SiteSettings.Add(new SiteSetting
        {
            Key = SiteSettingsService.MemberApplicationRecipientEmail,
            Value = configuration["EmailSettings:ToEmail"] ?? "",
            UpdatedAt = DateTime.Now
        });

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
