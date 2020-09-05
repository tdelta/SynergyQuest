/**
 * <summary>
 * We keep a file "Resources/ExternalArtCredits.yaml" which contains information (author, license etc.)
 * about every art asset (music, graphics, sounds, ...) which was not specifically made for this game
 * but sourced from platforms like OpenGameArt.org etc.
 *
 * This class describes the format of every entry of that file.
 * </summary>
 */
public class CreditsEntry
{
    public string author;
    public string description;
    public string license;
    public string modifications;
    public string link;
}
