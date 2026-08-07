import unittest
from pathlib import Path

from leather_nesting_demo import Piece, fill_leather, load_closed_lwpolylines, pack_pieces, parse_leather_size, polygons_overlap


class LeatherSizeTests(unittest.TestCase):
    def test_parses_width_and_height_in_millimetres(self):
        self.assertEqual(parse_leather_size("2000x1000"), (2000.0, 1000.0))
        self.assertEqual(parse_leather_size("2000X4000"), (2000.0, 4000.0))

    def test_rejects_malformed_or_non_positive_size(self):
        for value in ("2000", "x1000", "2000x0", "-1x1000"):
            with self.assertRaises(ValueError):
                parse_leather_size(value)


class DxfLoadingTests(unittest.TestCase):
    def test_loads_the_nine_closed_shoe_upper_outlines(self):
        pieces, ignored = load_closed_lwpolylines(Path("凉鞋.dxf"))

        self.assertEqual(len(pieces), 9)
        self.assertTrue(all(len(piece.points) >= 3 for piece in pieces))
        self.assertTrue(all(piece.area > 0 for piece in pieces))
        self.assertGreaterEqual(ignored["TEXT"], 18)


class PackingTests(unittest.TestCase):
    def test_identical_polygons_are_not_treated_as_non_overlapping(self):
        square = ((0, 0), (10, 0), (10, 10), (0, 10))

        self.assertTrue(polygons_overlap(square, square))

    def test_places_non_overlapping_squares_with_requested_gap(self):
        pieces = [
            Piece(1, ((0, 0), (10, 0), (10, 10), (0, 10)), 100),
            Piece(2, ((0, 0), (10, 0), (10, 10), (0, 10)), 100),
        ]

        result = pack_pieces(pieces, leather=(40, 20), gap_mm=5)

        self.assertEqual(len(result.placements), 2)
        self.assertEqual(len(result.unplaced), 0)
        self.assertGreaterEqual(result.placements[1].x - result.placements[0].x, 15)

    def test_leaves_a_piece_unplaced_when_it_cannot_fit(self):
        piece = Piece(1, ((0, 0), (30, 0), (30, 10), (0, 10)), 300)

        result = pack_pieces([piece], leather=(20, 20), gap_mm=0)

        self.assertEqual(result.placements, [])
        self.assertEqual([piece.index for piece in result.unplaced], [1])

    def test_repeats_available_sizes_until_the_leather_has_no_space(self):
        size = Piece(1, ((0, 0), (10, 0), (10, 10), (0, 10)), 100)

        result = fill_leather([size], leather=(25, 25), gap_mm=0, rotations=(0, 180))

        self.assertEqual(len(result.placements), 4)
        self.assertEqual(result.placed_counts, {1: 4})
        self.assertEqual(result.unplaced, [])
