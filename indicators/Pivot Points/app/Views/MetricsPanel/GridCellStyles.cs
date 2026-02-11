using cAlgo.API;

namespace cAlgo.Indicators
{
    public static class GridCellStyles
    {
        public static Style TitleStyle { get; private set; }
        public static Style TableHeaderStyle { get; private set; }
        public static Style LabelStyle { get; private set; }
        public static Style ValueStyle { get; private set; }
        public static Style PositiveStyle { get; private set; }
        public static Style NegativeStyle { get; private set; }

        private static bool _stylesCreated = false;

        public static void CreateStyles()
        {
            if (_stylesCreated)
                return;

            CreateTitleStyle();
            CreateTableHeaderStyle();
            CreateLabelStyle();
            CreateValueStyle();
            CreatePositiveStyle();
            CreateNegativeStyle();

            _stylesCreated = true;
        }

        private static void CreateTitleStyle()
        {
            TitleStyle = new Style();
            TitleStyle.Set(ControlProperty.FontFamily, "Bahnschrift");
            TitleStyle.Set(ControlProperty.FontSize, 11);
            TitleStyle.Set(ControlProperty.FontWeight, FontWeight.Bold);
            TitleStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF292929"));
            TitleStyle.Set(ControlProperty.ForegroundColor, Color.FromHex("FFB9B9B9"));
        }

        private static void CreateTableHeaderStyle()
        {
            TableHeaderStyle = new Style();
            TableHeaderStyle.Set(ControlProperty.FontFamily, "Bahnschrift");
            TableHeaderStyle.Set(ControlProperty.FontSize, 11);
            TableHeaderStyle.Set(ControlProperty.FontWeight, FontWeight.Bold);
            TableHeaderStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF3B3B3B"));
            TableHeaderStyle.Set(ControlProperty.ForegroundColor, Color.FromHex("FFB9B9B9"));
        }

        private static void CreateLabelStyle()
        {
            LabelStyle = new Style();
            LabelStyle.Set(ControlProperty.FontFamily, "Bahnschrift");
            LabelStyle.Set(ControlProperty.FontSize, 11);
            LabelStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF3B3B3B"));
            LabelStyle.Set(ControlProperty.ForegroundColor, Color.FromHex("FFB9B9B9"));
        }

        private static void CreateValueStyle()
        {
            ValueStyle = new Style();
            ValueStyle.Set(ControlProperty.FontFamily, "Bahnschrift");
            ValueStyle.Set(ControlProperty.FontSize, 10);
            ValueStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF292929"));
            ValueStyle.Set(ControlProperty.ForegroundColor, Color.FromHex("CCB9B9B9"));
        }

        private static void CreatePositiveStyle()
        {
            PositiveStyle = new Style();
            PositiveStyle.Set(ControlProperty.FontFamily, "Bahnschrift");
            PositiveStyle.Set(ControlProperty.FontSize, 10);
            PositiveStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF292929"));
            PositiveStyle.Set(ControlProperty.ForegroundColor, Color.FromHex("FF20B2AA"));
        }

        private static void CreateNegativeStyle()
        {
            NegativeStyle = new Style();
            NegativeStyle.Set(ControlProperty.FontFamily, "Bahnschrift");
            NegativeStyle.Set(ControlProperty.FontSize, 10);
            NegativeStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF292929"));
            NegativeStyle.Set(ControlProperty.ForegroundColor, Color.FromHex("FFFF7F50"));
        }

        public static Style GetValueStyle(bool isPositive, bool isNegative)
        {
            if (isPositive)
                return PositiveStyle;
            else if (isNegative)
                return NegativeStyle;
            else
                return ValueStyle;
        }
    }
}
