import sofa from "@/assets/product-sofa.jpg";
import bed from "@/assets/product-bed.jpg";
import chair from "@/assets/product-chair.jpg";
import table from "@/assets/product-table.jpg";
import wardrobe from "@/assets/product-wardrobe.jpg";
import lamp from "@/assets/product-lamp.jpg";
import shelf from "@/assets/product-shelf.jpg";

export type Category = "Sofas" | "Beds" | "Chairs" | "Tables" | "Wardrobes" | "Lighting" | "Storage";

export const CATEGORIES: Category[] = [
  "Sofas", "Beds", "Chairs", "Tables", "Wardrobes", "Lighting", "Storage",
];

export type Product = {
  id: string;
  name: string;
  category: Category;
  price: number;
  image: string;
  description: string;
  features: string[];
};

export const PRODUCTS: Product[] = [
  {
    id: "linen-sofa",
    name: "Halden Linen Sofa",
    category: "Sofas",
    price: 1290,
    image: sofa,
    description:
      "A three-seat sofa upholstered in heavy washed linen with solid oak legs. Designed for long, lazy afternoons.",
    features: ["Solid oak legs", "Washed linen cover", "Removable cushions", "Made in Denmark"],
  },
  {
    id: "oak-bed",
    name: "Sønder Oak Bed",
    category: "Beds",
    price: 980,
    image: bed,
    description:
      "Solid white oak platform bed with a low headboard. Honest joinery and a soft, lived-in finish.",
    features: ["Solid white oak", "Slatted base included", "Queen / King sizes", "Hand-finished"],
  },
  {
    id: "boucle-chair",
    name: "Lykke Boucle Lounge Chair",
    category: "Chairs",
    price: 540,
    image: chair,
    description:
      "A curved lounge chair wrapped in soft boucle. The kind you sink into and forget the time.",
    features: ["Boucle upholstery", "Light oak legs", "Foam-and-feather seat"],
  },
  {
    id: "round-table",
    name: "Hagen Round Dining Table",
    category: "Tables",
    price: 870,
    image: table,
    description:
      "A round oak dining table with matching chairs. Friendly geometry for four.",
    features: ["Solid oak top", "Includes 4 chairs", "Seats up to 4"],
  },
  {
    id: "oak-wardrobe",
    name: "Frej Sliding Wardrobe",
    category: "Wardrobes",
    price: 1450,
    image: wardrobe,
    description:
      "Tall oak wardrobe with smooth sliding doors and a bottom drawer. Quiet and generous.",
    features: ["Soft-close doors", "Internal hanging rail", "Lower drawer"],
  },
  {
    id: "arc-lamp",
    name: "Bue Arc Floor Lamp",
    category: "Lighting",
    price: 320,
    image: lamp,
    description:
      "An elegant brass arc with a hand-stitched linen shade. Pours warm light over a reading nook.",
    features: ["Brushed brass arm", "Linen drum shade", "Foot dimmer"],
  },
  {
    id: "walnut-shelf",
    name: "Bjørk Walnut Bookshelf",
    category: "Storage",
    price: 690,
    image: shelf,
    description:
      "A staggered walnut shelf for books, ceramics, and the things you keep coming back to.",
    features: ["Solid walnut", "5 open shelves", "Wall-anchor included"],
  },
  {
    id: "linen-armchair",
    name: "Halden Armchair",
    category: "Sofas",
    price: 690,
    image: chair,
    description: "The single-seat companion to the Halden sofa. Same linen, same warmth.",
    features: ["Linen cover", "Oak legs", "Removable cushion"],
  },
];

export const findProduct = (id: string) => PRODUCTS.find((p) => p.id === id);
