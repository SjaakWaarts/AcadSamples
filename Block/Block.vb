Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Runtime
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.ApplicationServices
Imports DBTransMan = Autodesk.AutoCAD.DatabaseServices.TransactionManager

<Assembly: Autodesk.AutoCAD.Runtime.ExtensionApplication(Nothing)>
<Assembly: Autodesk.AutoCAD.Runtime.CommandClass(GetType(CreateEntities.MakeEntities))>

'This application implements a command called MKENTS. It will create a line and 
'a circle, append them to an object ID array, change the circle's color to red,
'and then make a group of the line and circle using "ASDK_TEST_GROUP" as the 
'group's name. The MKENTS command also creates a new layer named "ASDK_MYLAYER".
'
'To use Ents.dll:
'
'1. Start AutoCAD and open a new drawing.
'2. Type netload and select Ents.dll.
'3. Execute the MKENTS command.
'
'Autodesk references added to this project are the acdbmgd.dll and acmgd.dll .NET components,
'and the AutoCAD Type Library COM component.

Namespace CreateEntities



    Public Class MakeEntities

        <CommandMethod("InsertingBlockWithAnAttribute")>
        Public Sub InsertingBlockWithAnAttribute()
            ' Get the current database and start a transaction
            Dim acCurDb As Autodesk.AutoCAD.DatabaseServices.Database
            acCurDb = Application.DocumentManager.MdiActiveDocument.Database

            Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
                ' Open the Block table for read
                Dim acBlkTbl As BlockTable
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

                Dim blkRecId As ObjectId = ObjectId.Null

                If Not acBlkTbl.Has("CircleBlockWithAttributes") Then
                    Using acBlkTblRec As New BlockTableRecord
                        acBlkTblRec.Name = "CircleBlockWithAttributes"

                        ' Set the insertion point for the block
                        acBlkTblRec.Origin = New Point3d(0, 0, 0)

                        ' Add a circle to the block
                        Using acCirc As New Circle
                            acCirc.Center = New Point3d(0, 0, 0)
                            acCirc.Radius = 2

                            acBlkTblRec.AppendEntity(acCirc)

                            ' Add an attribute definition to the block
                            Using acAttDef As New AttributeDefinition
                                acAttDef.Position = New Point3d(0, 0, 0)
                                acAttDef.Prompt = "Door #: "
                                acAttDef.Tag = "Door#"
                                acAttDef.TextString = "DXX"
                                acAttDef.Height = 1
                                acAttDef.Justify = AttachmentPoint.MiddleCenter
                                acBlkTblRec.AppendEntity(acAttDef)

                                acBlkTbl.UpgradeOpen()
                                acBlkTbl.Add(acBlkTblRec)
                                acTrans.AddNewlyCreatedDBObject(acBlkTblRec, True)
                            End Using

                            blkRecId = acBlkTblRec.Id
                        End Using
                    End Using
                Else
                    blkRecId = acBlkTbl("CircleBlockWithAttributes")
                End If

                ' Create and insert the new block reference
                If blkRecId <> ObjectId.Null Then
                    Dim acBlkTblRec As BlockTableRecord
                    acBlkTblRec = acTrans.GetObject(blkRecId, OpenMode.ForRead)

                    Using acBlkRef As New BlockReference(New Point3d(2, 2, 0), acBlkTblRec.Id)

                        Dim acCurSpaceBlkTblRec As BlockTableRecord
                        acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite)

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef)
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, True)

                        ' Verify block table record has attribute definitions associated with it
                        If acBlkTblRec.HasAttributeDefinitions Then
                            ' Add attributes from the block table record
                            For Each objID As ObjectId In acBlkTblRec

                                Dim dbObj As DBObject = acTrans.GetObject(objID, OpenMode.ForRead)

                                If TypeOf dbObj Is AttributeDefinition Then
                                    Dim acAtt As AttributeDefinition = dbObj

                                    If Not acAtt.Constant Then
                                        Using acAttRef As New AttributeReference

                                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform)
                                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform)

                                            acAttRef.TextString = acAtt.TextString

                                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef)

                                            acTrans.AddNewlyCreatedDBObject(acAttRef, True)
                                        End Using
                                    End If
                                End If
                            Next
                        End If
                    End Using
                End If

                ' Save the new object to the database
                acTrans.Commit()

                ' Dispose of the transaction
            End Using
        End Sub


        <CommandMethod("MKENTS")>
        Public Sub runit()
            Try
                Call createNewLayer()
                Dim coll As New ObjectIdCollection()

                coll.Add(CreateCircle())
                coll.Add(CreateLine())
                Dim last As Integer
                last = coll.Count
                changeColor(coll.Item(last - 1), 1)
                createGroup(coll, "ASDK_TEST_GROUP")
            Catch ex As System.Exception
                MsgBox(ex.Message)
            Finally
            End Try
        End Sub

        Public Sub createNewLayer()
            Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
            Dim tm As DBTransMan = db.TransactionManager
            Dim ta As Transaction = tm.StartTransaction()
            Try
                Dim LT As LayerTable = tm.GetObject(db.LayerTableId, OpenMode.ForRead, False)
                If LT.Has("ASDK_MYLAYER") = False Then
                    Dim LTRec As New LayerTableRecord()
                    LTRec.Name = "ASDK_MYLAYER"
                    LT.UpgradeOpen()
                    LT.Add(LTRec)
                    tm.AddNewlyCreatedDBObject(LTRec, True)
                    ta.Commit()
                End If
            Finally
                ta.Dispose()
            End Try
        End Sub

        Public Function CreateLine() As ObjectId
            Dim startpt As New Point3d(4.0, 2.0, 0.0)
            Dim endpt As New Point3d(10.0, 7.0, 0.0)
            Dim pLine As New Line(startpt, endpt)

            Dim lineid As ObjectId
            Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
            Dim tm As DBTransMan = db.TransactionManager
            Dim ta As Transaction = tm.StartTransaction()
            Try
                Dim bt As BlockTable = tm.GetObject(db.BlockTableId, OpenMode.ForRead, False)
                Dim btr As BlockTableRecord = tm.GetObject(bt(BlockTableRecord.ModelSpace), OpenMode.ForWrite, False)
                lineid = btr.AppendEntity(pLine)
                tm.AddNewlyCreatedDBObject(pLine, True)
                ta.Commit()
            Finally
                ta.Dispose()
            End Try
            Return lineid
        End Function

        Public Function CreateCircle() As ObjectId
            Dim center As New Point3d(9.0, 3.0, 0.0)
            Dim normal As New Vector3d(0.0, 0.0, 1.0)
            Dim pcirc As New Circle(center, normal, 2.0)
            Dim Circid As ObjectId

            Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
            Dim tm As DBTransMan = db.TransactionManager
            'start a transaction
            Dim ta As Transaction = tm.StartTransaction
            Try
                Dim bt As BlockTable = tm.GetObject(db.BlockTableId, OpenMode.ForRead, False)
                Dim btr As BlockTableRecord = tm.GetObject(bt(BlockTableRecord.ModelSpace), OpenMode.ForWrite, False)
                Circid = btr.AppendEntity(pcirc)
                tm.AddNewlyCreatedDBObject(pcirc, True)
                ta.Commit()
            Finally
                ta.Dispose()
            End Try
            Return Circid
        End Function

        Private Sub changeColor(ByVal entId As ObjectId, ByVal newColor As Long)
            Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
            Dim tm As DBTransMan = db.TransactionManager
            'start a transaction
            Dim ta As Transaction = tm.StartTransaction()
            Try
                Dim ent As Entity = tm.GetObject(entId, OpenMode.ForWrite, True)
                ent.ColorIndex = newColor
                ta.Commit()
            Catch
                Console.WriteLine("Error in setting the color for the entity")
            Finally
                ta.Dispose()
            End Try
        End Sub

        Private Sub createGroup(ByVal objIds As ObjectIdCollection, ByVal pGroupName As System.String)
            Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
            Dim tm As DBTransMan = db.TransactionManager
            'start a transaction
            Dim ta As Transaction = tm.StartTransaction()
            Try
                Dim gp As New Group(pGroupName, True)
                Dim dict As DBDictionary = tm.GetObject(db.GroupDictionaryId, OpenMode.ForWrite, True)
                dict.SetAt("ASDK_NEWNAME", gp)
                Dim thisId As ObjectId
                For Each thisId In objIds
                    gp.Append(thisId)
                Next
                tm.AddNewlyCreatedDBObject(gp, True)
                ta.Commit()
            Finally
                ta.Dispose()
            End Try
        End Sub
    End Class

End Namespace
